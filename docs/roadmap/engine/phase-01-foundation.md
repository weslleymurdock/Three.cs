# Engine Phase 01 — Foundation

## Checklist

- [ ] Remove only the existing `Class1.cs` placeholder.
- [ ] Define engine options and lifecycle/init contracts.
- [ ] Define renderer/backend abstractions independent of MAUI.
- [ ] Define scene and resource ownership contracts.
- [ ] Define cancellation and shutdown semantics.
- [ ] Add DI registration extensions without concrete host coupling.
- [ ] Add lifecycle and disposal tests.
- [ ] Validate the solution before checking items.

## Exit criteria

The engine has clean host-independent contracts and lifecycle semantics, with no premature renderer or platform implementation.