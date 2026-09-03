using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archie.Scanner.Contracts;

public enum ObservationSource { Authored, Scanner }

public sealed record ScannerIdentity(string Id, string Version);

public sealed record EntityCandidate(
    string Key,
    NodeKind Kind,
    string? ExplicitId,
    string Name,
    Resolution Resolution,
    IReadOnlyDictionary<string, string> IdentitySignals,
    IReadOnlyDictionary<string, JsonElement> Properties);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(EntityObservation), "entity")]
[JsonDerivedType(typeof(RelationshipObservation), "relationship")]
public abstract record Observation(string Id, Evidence Evidence);

public sealed record EntityObservation(
    string Id,
    Evidence Evidence,
    EntityCandidate Entity) : Observation(Id, Evidence);

public sealed record RelationshipObservation(
    string Id,
    Evidence Evidence,
    EdgeKind Relationship,
    EntityCandidate From,
    EntityCandidate To,
    IReadOnlyDictionary<string, JsonElement> Properties) : Observation(Id, Evidence);

public sealed record ObservationBundle(
    string SchemaVersion,
    ObservationSource Source,
    string ScanConfigurationDigest,
    RepositoryRevision Repository,
    IReadOnlyList<ScannerIdentity> Scanners,
    IReadOnlyList<Observation> Observations,
    IReadOnlyList<Diagnostic> Diagnostics,
    IReadOnlyList<RedactionEvent> Redactions);
