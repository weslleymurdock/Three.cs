---
name: three-engine-agent
---
# Three.Engine Agent Skill

Use this skill for engine implementation work.

## Required reading

Read `docs/roadmap/ENGINE-ROADMAP.md`, the active checklist under `docs/roadmap/engine/`, and `docs/GUIDELINES.md`.

## Rules

- Keep the engine host/platform independent.
- Treat `Three.cs` as the Three.js runtime boundary.
- Use explicit lifecycle, ownership, cancellation, and disposal contracts.
- Never implement a frame loop with one unbounded task per frame.
- Keep update/simulation separate from render submission.
- Introduce backend abstractions before concrete backend coupling.
- Validate repeated start/stop/dispose and failure paths.

## Completion report

Report architecture decisions, changed files, tests/benchmarks, runtime validation, and unresolved risks.