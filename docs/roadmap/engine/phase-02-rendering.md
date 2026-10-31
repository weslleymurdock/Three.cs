# Engine Phase 02 — Rendering

## Checklist

- [ ] Define GPU/device/surface/command abstractions.
- [ ] Define the WGPU-inspired capability model.
- [ ] Map supported Three.js/WebGPU concepts to the engine boundary.
- [ ] Implement the first Three.cs-backed renderer through abstractions.
- [ ] Define ownership for buffers, textures, pipelines, and render targets.
- [ ] Define GPU-unavailable failure behavior.
- [ ] Add backend lifecycle and capability tests.
- [ ] Validate initialization and a first render submission path.

## Exit criteria

A backend can be selected and initialized through interfaces and the first Three.cs-backed renderer is stable enough for the frame loop phase.