# Contributing

Contributions are welcome through GitHub pull requests.

By contributing, you certify the Developer Certificate of Origin 1.1. Add a sign-off to every commit with `git commit -s`; the sign-off records that you have the right to submit the contribution under this repository's license.

Before opening a pull request, run:

```bash
dotnet restore --locked-mode
dotnet test --no-restore
```

Protocol changes must update the schemas, documentation, valid and invalid fixtures, and .NET binding tests together. Closed objects and exact protocol identifiers mean most new fields or tightened validation require a new protocol version.
