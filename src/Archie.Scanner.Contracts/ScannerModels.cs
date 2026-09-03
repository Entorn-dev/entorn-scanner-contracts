using System.Text.Json;

namespace Archie.Scanner.Contracts;

public enum NodeKind
{
    System,
    Domain,
    Repository,
    Deployable,
    Module,
    Component,
    HttpEndpoint,
    RpcEndpoint,
    MessageChannel,
    EventContract,
    ApiContract,
    Database,
    Cache,
    ObjectStore,
    ExternalService,
    InfrastructureResource,
    Team
}

public enum EdgeKind
{
    Contains,
    References,
    Exposes,
    Calls,
    Publishes,
    Subscribes,
    ReadsFrom,
    WritesTo,
    Implements,
    UsesContract,
    DeploysTo,
    OwnedBy,
    DependsOn
}

public enum EvidenceProvenance { Deterministic, Manual, Ai, Runtime }

public enum Confidence { Confirmed, Inferred }

public enum Resolution { Resolved, Unresolved, Ambiguous }

public sealed record RepositoryRevision(
    string RepositoryId,
    string? RemoteUrl,
    string Revision,
    bool IsDirty,
    string ContentDigest);

public sealed record SourceRange(int StartLine, int StartColumn, int EndLine, int EndColumn);

public sealed record Evidence(
    string Id,
    string ClaimId,
    EvidenceProvenance Provenance,
    string? ScannerId,
    string? ScannerVersion,
    string ExtractionMethod,
    string Path,
    SourceRange? Range,
    Confidence Confidence,
    IReadOnlyDictionary<string, JsonElement> Properties);

public sealed record Diagnostic(string Id, string Code, string Severity, string Message, string? SubjectId);

public sealed record RedactionEvent(string Id, string Kind, string Path, string Message);
