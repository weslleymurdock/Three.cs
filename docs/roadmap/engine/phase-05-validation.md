# Engine Phase 05 — Validation

## Checklist

- [ ] Create a minimal Blazor WASM engine consumer.
- [ ] Validate Android MAUI Hybrid.
- [ ] Validate iOS MAUI Hybrid.
- [ ] Validate MacCatalyst MAUI Hybrid.
- [ ] Validate Windows MAUI Hybrid.
- [ ] Validate startup, shutdown, cancellation, and frame-loop behavior.
- [ ] Validate renderer capability failure and diagnostics.
- [ ] Add CI coverage for supported hosts/TFMs.
- [ ] Validate packaging from a clean consumer.
- [ ] Record evidence before checking items.

## Exit criteria

The engine can be hosted through DI, run its lifecycle and frame loop, and cleanly report unsupported backend capabilities across supported hosts.