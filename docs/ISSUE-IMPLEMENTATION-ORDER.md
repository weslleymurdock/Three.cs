# Implementation Issue Order

This document is the canonical execution order for generating implementation prompts and implementing the two project roadmaps.

The order deliberately completes the `Three.cs` port before implementation of `Three.Engine`, because the engine is layered on top of the public runtime and interop abstractions delivered by `Three.cs`. Do not start an engine phase while any preceding issue in this sequence remains incomplete.

## Feature issues

- [[FEATURE] Three.cs](https://github.com/weslleymurdock/Three.cs/issues/2) — Three.js runtime port, C# bindings, and Blazor/MAUI Hybrid hosting.
- [[FEATURE] Three.Engine](https://github.com/weslleymurdock/Three.cs/issues/3) — platform-independent engine architecture built on Three.cs.

## Global execution sequence

Generate one implementation prompt at a time, following this list strictly. Before generating the next prompt, verify that the current task has been implemented and its acceptance criteria validated. Update roadmap checklists only when there is concrete implementation and validation evidence.

### Track 1 — Three.cs

Parent: [[FEATURE] Three.cs](https://github.com/weslleymurdock/Three.cs/issues/2)

1. [[TASK] Three.cs — Foundation](https://github.com/weslleymurdock/Three.cs/issues/4)  
   Normalize and validate the existing RCL and repository foundations. Preserve the solution, both projects, project reference, and pinned Three.js submodule.
2. [[TASK] Three.cs — Three.js Runtime](https://github.com/weslleymurdock/Three.cs/issues/5)  
   Integrate the pinned Three.js runtime and stable static asset/module loading boundary.
3. [[TASK] Three.cs — C# Bindings and MAUI Bridge](https://github.com/weslleymurdock/Three.cs/issues/6)  
   Implement typed C# bindings, JS interop lifecycle, and MAUI Hybrid WebView bridge.
4. [[TASK] Three.cs — Cross-Host Validation and Packaging](https://github.com/weslleymurdock/Three.cs/issues/7)  
   Validate Blazor WASM and MAUI Hybrid targets, samples, CI, and consumer packaging.

### Track 2 — Three.Engine

Parent: [[FEATURE] Three.Engine](https://github.com/weslleymurdock/Three.cs/issues/3)

Start this track only after Track 1 has been completed and the Three.cs public API/runtime boundary is stable.

5. [[TASK] Three.Engine — Foundation](https://github.com/weslleymurdock/Three.cs/issues/8)  
   Define host-independent engine options, lifecycle, renderer/backend contracts, ownership, cancellation, and DI registration.
6. [[TASK] Three.Engine — Rendering Abstractions](https://github.com/weslleymurdock/Three.cs/issues/9)  
   Establish WGPU-inspired rendering abstractions and the first Three.cs-backed renderer.
7. [[TASK] Three.Engine — Deterministic Frame Loop](https://github.com/weslleymurdock/Three.cs/issues/10)  
   Implement deterministic timing, cancellable scheduling, and explicit update/render sequencing.
8. [[TASK] Three.Engine — Runtime, Scene and Resource Lifecycle](https://github.com/weslleymurdock/Three.cs/issues/11)  
   Integrate scenes, resources, platform-independent input, engine lifecycle, and deterministic cleanup.
9. [[TASK] Three.Engine — Cross-Platform Validation and Packaging](https://github.com/weslleymurdock/Three.cs/issues/12)  
   Validate the full engine lifecycle on browser and MAUI Hybrid hosts, CI, and consumer packaging.

## Rules for prompt generation and execution

- Read the parent feature issue, task issue, relevant roadmap, phase checklist, and `docs/GUIDELINES.md` before generating each prompt.
- Prompts must be scoped to exactly one task issue and must preserve all architecture and repository constraints.
- Do not silently reorder, combine, or skip tasks. If a dependency blocks progress, document it in the relevant issue before proceeding.
- Do not mark a task complete or check roadmap items merely because code or tests were added; validate the behavior and record evidence.
- Never claim a target platform was validated unless the corresponding build/test/run actually executed.
- The GitHub `feature` and `task` labels are applied to the issues. The repository owner can subsequently assign native GitHub issue types and establish native parent/sub-issue relationships using the Markdown references above.

## Roadmap sources

- [Three.cs Port Roadmap](roadmap/PORT-ROADMAP.md)
- [Three.Engine Roadmap](roadmap/ENGINE-ROADMAP.md)
- [Three.cs phase checklists](roadmap/port/)
- [Three.Engine phase checklists](roadmap/engine/)
- [Repository and implementation guidelines](GUIDELINES.md)
