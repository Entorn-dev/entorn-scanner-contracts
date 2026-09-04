using System.Diagnostics;
using System.Text.Json.Nodes;
using Entorn.Scanner.Contracts;
using Json.Schema;
using Xunit;

namespace Entorn.Scanner.Contracts.Tests;

public sealed class ProtocolConformanceTests
{
    [Fact]
    public void FixturesHaveExpectedSchemaValidity()
    {
        var (schema, options) = ProtocolSchema();
        var fixtureRoot = Path.Combine(AppContext.BaseDirectory, "fixtures", "protocol");

        foreach (var path in Directory.EnumerateFiles(Path.Combine(fixtureRoot, "valid"), "*.json"))
        {
            var result = schema.Evaluate(JsonNode.Parse(File.ReadAllText(path))!, options);
            Assert.True(result.IsValid, $"Expected {Path.GetFileName(path)} to be valid: {result}");
        }

        foreach (var path in Directory.EnumerateFiles(Path.Combine(fixtureRoot, "invalid"), "*.json"))
        {
            var result = schema.Evaluate(JsonNode.Parse(File.ReadAllText(path))!, options);
            Assert.False(result.IsValid, $"Expected {Path.GetFileName(path)} to be invalid.");
        }
    }

    [Fact]
    public void BindingReadsEveryValidProtocolFixture()
    {
        var fixtureRoot = Path.Combine(AppContext.BaseDirectory, "fixtures", "protocol", "valid");

        foreach (var path in Directory.EnumerateFiles(fixtureRoot, "*.json"))
        {
            var message = System.Text.Json.JsonSerializer.Deserialize<ProtocolMessage>(
                File.ReadAllText(path), ScannerContractJson.Options);
            Assert.NotNull(message);
            Assert.Equal("scanner/v1", message.ProtocolVersion);
        }
    }

    [Fact]
    public async Task LanguageNeutralWorkerEmitsSchemaAndBindingCompatibleMessages()
    {
        var fixtureRoot = Path.Combine(AppContext.BaseDirectory, "fixtures", "protocol");
        var start = new ProcessStartInfo("node", [Path.Combine(fixtureRoot, "non-dotnet-worker.mjs")])
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var process = Process.Start(start)
            ?? throw new InvalidOperationException("Could not start the fixture worker.");
        var lines = new List<string> { (await process.StandardOutput.ReadLineAsync())! };
        var request = JsonNode.Parse(await File.ReadAllTextAsync(
            Path.Combine(fixtureRoot, "valid", "scan-request.json")))!;
        await process.StandardInput.WriteLineAsync(request.ToJsonString());
        process.StandardInput.Close();
        while (await process.StandardOutput.ReadLineAsync() is { } line) lines.Add(line);
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(0, process.ExitCode);
        Assert.Equal(6, lines.Count);
        var (schema, options) = ProtocolSchema();
        var messages = lines.Select(line =>
        {
            Assert.True(schema.Evaluate(JsonNode.Parse(line)!, options).IsValid);
            return System.Text.Json.JsonSerializer.Deserialize<ProtocolMessage>(line, ScannerContractJson.Options)!;
        }).ToArray();
        Assert.IsType<ReadyMessage>(messages[0]);
        Assert.Equal(2, messages.OfType<ObservationMessage>().Count());
        Assert.Single(messages.OfType<SourceOwnershipMessage>());
        Assert.Single(messages.OfType<DiagnosticMessage>());
        Assert.Equal(2, Assert.Single(messages.OfType<CompletedMessage>()).Summary.ObservationCount);
    }

    [Fact]
    public void BindingHasNoEntornCoreDependency()
    {
        var references = typeof(ProtocolMessage).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(references, reference =>
            reference.Name is "Archie.Contracts" or "Archie.Core" or "Entorn.Contracts" or "Entorn.Core");
    }

    private static (JsonSchema Schema, EvaluationOptions Options) ProtocolSchema()
    {
        var schemaRoot = Path.Combine(AppContext.BaseDirectory, "schemas", "v1");
        var options = new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true };
        foreach (var path in Directory.EnumerateFiles(schemaRoot, "*.schema.json"))
            options.SchemaRegistry.Register(JsonSchema.FromText(File.ReadAllText(path)));
        return (JsonSchema.FromText(File.ReadAllText(
            Path.Combine(schemaRoot, "protocol-message.schema.json"))), options);
    }
}
