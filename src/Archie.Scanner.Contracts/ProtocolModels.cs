using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archie.Scanner.Contracts;

public sealed record ScannerPermissions(bool ReadRepository, bool Network, bool Environment);

public sealed record ScannerManifest(
    string SchemaVersion,
    string Id,
    string Version,
    string Executable,
    IReadOnlyList<string> Arguments,
    IReadOnlyList<string> ArtifactGlobs,
    IReadOnlyList<string> Capabilities,
    JsonElement ConfigurationSchema,
    ScannerPermissions Permissions);

public sealed record ScanContext(
    RepositoryRevision Repository,
    string CheckoutPath,
    JsonElement Configuration);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ReadyMessage), "ready")]
[JsonDerivedType(typeof(ScanRequestMessage), "scan-request")]
[JsonDerivedType(typeof(ObservationMessage), "observation")]
[JsonDerivedType(typeof(SourceOwnershipMessage), "source-ownership")]
[JsonDerivedType(typeof(DiagnosticMessage), "diagnostic")]
[JsonDerivedType(typeof(CompletedMessage), "completed")]
public abstract record ProtocolMessage(string ProtocolVersion);

public sealed record ReadyMessage(
    string ProtocolVersion,
    ScannerIdentity Scanner) : ProtocolMessage(ProtocolVersion);

public sealed record ScanRequestMessage(
    string ProtocolVersion,
    ScanContext Context) : ProtocolMessage(ProtocolVersion);

public sealed record ObservationMessage(
    string ProtocolVersion,
    Observation Observation) : ProtocolMessage(ProtocolVersion);

public sealed record SourceOwnershipMessage(
    string ProtocolVersion,
    SourceOwnershipClaim Ownership) : ProtocolMessage(ProtocolVersion);

public sealed record DiagnosticMessage(
    string ProtocolVersion,
    Diagnostic Diagnostic) : ProtocolMessage(ProtocolVersion);

public sealed record ScanSummary(int ObservationCount);

public sealed record CompletedMessage(
    string ProtocolVersion,
    ScanSummary Summary) : ProtocolMessage(ProtocolVersion);
