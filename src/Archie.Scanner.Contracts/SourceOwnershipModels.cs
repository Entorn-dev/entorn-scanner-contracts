namespace Archie.Scanner.Contracts;

public enum SourceOwnershipKind { Project, Module, Component, Deployable }

public sealed record SourceOwnershipClaim(
    string ScannerId,
    string ScannerVersion,
    string Path,
    string OwnerCandidateKey,
    SourceOwnershipKind OwnershipKind,
    Confidence Confidence,
    Resolution Resolution,
    string DerivationRule);
