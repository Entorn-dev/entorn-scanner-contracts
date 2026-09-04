# Scanner protocol

Entorn scanners are separate processes that exchange newline-delimited JSON over standard input and output. The authoritative contract is the JSON Schema subset in `schemas/v1`: `common.schema.json`, `scanner-manifest.schema.json`, `observation-bundle.schema.json`, and `protocol-message.schema.json`. The fixtures in `fixtures/protocol` provide valid and invalid language-neutral examples. `Entorn.Scanner.Contracts` is an optional .NET binding, not the source of truth.

## Lifecycle

1. Entorn launches the manifest's executable directly, without a shell, using the approved containment and resource limits.
2. The worker writes exactly one `ready` message.
3. Entorn writes exactly one `scan-request` message.
4. The worker writes zero or more `observation`, `source-ownership`, and `diagnostic` messages.
5. The worker writes exactly one terminal `completed` message and exits successfully.

Every message is one UTF-8 JSON object followed by a newline and has `protocolVersion: "scanner/v1"`. Standard output is protocol-only. Human logs may use standard error; Entorn sanitizes them before turning failures into diagnostics.

`completed.summary.observationCount` counts only `observation` messages. Source-ownership and diagnostic messages do not contribute to it. A worker must finish discovery before writing output when it cannot guarantee a valid bounded stream; Entorn rejects malformed, oversized, out-of-order, or incomplete streams atomically.

## Authority boundary

Scanners report evidence-backed entity and relationship candidates, diagnostics, and source-ownership claims. They do not assign canonical graph identities, merge candidates, reconcile conflicting evidence, redact persisted output, or publish graph snapshots. Entorn validates and redacts scanner output, reconciles candidates, and remains the sole authority for canonical graph and source-context artifacts.

## Manifest

`scanner-manifest/v1` identifies the worker and declares its executable, argument vector, applicability globs, capabilities, configuration schema, and permissions. Entorn currently accepts repository-read access only; network and inherited-environment permissions must be false. Manifest applicability selects installed workers but does not grant a scanner authority over canonical results.

## Implementing in another language

A scanner written in PHP, Go, TypeScript, Python, or another language should validate its manifest and emitted messages directly against the schemas and run the conformance fixtures. It does not need .NET or NuGet. The Node.js fixture worker at `fixtures/protocol/non-dotnet-worker.mjs` demonstrates the complete exchange without consuming the .NET binding.

See [scanner compatibility](compatibility.md) before declaring supported protocol versions.
