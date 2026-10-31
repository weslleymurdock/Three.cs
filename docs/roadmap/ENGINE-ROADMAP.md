# Three.Engine Roadmap

`Three.Engine` is the platform-independent engine layer built on top of `Three.cs`. It adopts useful WGPU concepts while keeping rendering, lifecycle, scheduling, input, and resource ownership explicit and testable.

## Phases

1. Foundation — engine contracts, lifecycle, DI, cancellation, ownership.
2. Rendering — device/surface/command abstractions and the first Three.cs-backed renderer.
3. Frame loop — deterministic timing and cancellable scheduling.
4. Runtime — scene/resource/input/lifecycle integration.
5. Validation — browser and MAUI host validation, CI, packaging.

## Constraints

- No platform-specific implementation in the core engine.
- No blocking waits or unbounded per-frame tasks.
- Rendering backend selection occurs through abstractions.
- Resource ownership and disposal order are explicit.
- `Three.cs` remains the JavaScript/Three.js boundary; `Three.Engine` consumes it instead of duplicating it.

## Definition of done

A host can construct the engine through DI, initialize a backend, attach a scene, run and stop a deterministic frame loop, and dispose all owned resources correctly across supported browser and MAUI targets.

Each phase has a dedicated checklist under `docs/roadmap/engine/`.