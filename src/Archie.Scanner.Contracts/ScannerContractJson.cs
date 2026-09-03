using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archie.Scanner.Contracts;

public static class ScannerContractJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static ObservationBundle ReadObservationBundle(Stream stream) =>
        JsonSerializer.Deserialize<ObservationBundle>(stream, Options)
            ?? throw new JsonException("The observation bundle is empty.");

    public static byte[] WriteObservationBundle(ObservationBundle bundle)
    {
        var normalized = bundle with
        {
            Scanners = bundle.Scanners.OrderBy(scanner => scanner.Id, StringComparer.Ordinal).ToArray(),
            Observations = bundle.Observations.Select(Normalize).OrderBy(item => item.Id, StringComparer.Ordinal).ToArray(),
            Diagnostics = bundle.Diagnostics.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray(),
            Redactions = bundle.Redactions.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray()
        };
        return JsonSerializer.SerializeToUtf8Bytes(normalized, Options);
    }

    private static Observation Normalize(Observation observation) => observation switch
    {
        EntityObservation entity => entity with
        {
            Evidence = Normalize(entity.Evidence),
            Entity = Normalize(entity.Entity)
        },
        RelationshipObservation relationship => relationship with
        {
            Evidence = Normalize(relationship.Evidence),
            From = Normalize(relationship.From),
            To = Normalize(relationship.To),
            Properties = SortProperties(relationship.Properties)
        },
        _ => throw new InvalidOperationException($"Unsupported observation type {observation.GetType().Name}.")
    };

    private static EntityCandidate Normalize(EntityCandidate candidate) => candidate with
    {
        IdentitySignals = new SortedDictionary<string, string>(
            candidate.IdentitySignals.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            StringComparer.Ordinal),
        Properties = SortProperties(candidate.Properties)
    };

    private static Evidence Normalize(Evidence evidence) => evidence with
    {
        Properties = SortProperties(evidence.Properties)
    };

    private static IReadOnlyDictionary<string, JsonElement> SortProperties(IReadOnlyDictionary<string, JsonElement> properties) =>
        new SortedDictionary<string, JsonElement>(
            properties.ToDictionary(pair => pair.Key, pair => NormalizeJson(pair.Value), StringComparer.Ordinal),
            StringComparer.Ordinal);

    private static JsonElement NormalizeJson(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => JsonSerializer.SerializeToElement(
            value.EnumerateObject().ToDictionary(
                property => property.Name,
                property => NormalizeJson(property.Value),
                StringComparer.Ordinal)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)),
        JsonValueKind.Array => JsonSerializer.SerializeToElement(value.EnumerateArray().Select(NormalizeJson).ToArray()),
        _ => value.Clone()
    };

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowOutOfOrderMetadataProperties = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower, allowIntegerValues: false));
        return options;
    }
}
