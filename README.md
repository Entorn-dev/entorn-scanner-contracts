# Entorn scanner contracts

This repository is the language-neutral source of truth for Entorn's scanner protocol. Versioned JSON Schemas, protocol documentation, and conformance fixtures define the wire contract. Scanners can be implemented in any language without using .NET or NuGet.

`Entorn.Scanner.Contracts` is the optional official .NET binding. It mirrors the schemas but does not extend them.

## Validate

```bash
dotnet restore --locked-mode
dotnet test --no-restore
```

See [the protocol](docs/protocol.md) and [compatibility policy](docs/compatibility.md) before implementing a scanner.

## Compatibility note

The first public release preserves `scanner/v1`, its existing schema identifiers, and existing `archie.*` scanner identities byte-for-byte while the wider product rename proceeds separately. Changing those wire identities would be a protocol migration rather than a repository rename.

## License and contributions

Licensed under Apache-2.0. Contributions require a Developer Certificate of Origin sign-off; see [CONTRIBUTING.md](CONTRIBUTING.md).
