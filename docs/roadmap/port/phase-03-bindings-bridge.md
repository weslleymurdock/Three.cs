# Port Phase 03 — Bindings and MAUI Bridge

## Checklist

- [ ] Define typed C# object and lifecycle contracts.
- [ ] Implement a shared JS interop transport for browser and MAUI hosts.
- [ ] Keep initialization/invocation asynchronous and cancellable.
- [ ] Implement MAUI WebView bridge injection and readiness detection.
- [ ] Make injection idempotent and tied to WebView lifecycle.
- [ ] Dispose JS handles and bridge resources deterministically.
- [ ] Validate cancellation, readiness, invocation failure, and disposal paths.
- [ ] Validate the same supported flow in Blazor WASM.
- [ ] Record evidence before marking checklist items complete.

## Exit criteria

C# can create and dispose the first supported Three.js objects through the same abstraction in Blazor WASM and MAUI Hybrid.