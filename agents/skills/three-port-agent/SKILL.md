---
name: three-port-agent
---
# Three.cs Port Agent Skill

Use this skill for work under the port roadmap.

## Required reading

Read `docs/roadmap/PORT-ROADMAP.md`, the active phase checklist under `docs/roadmap/port/`, and `docs/GUIDELINES.md` before changing code.

## Rules

- Inspect the current repository before editing.
- Preserve the existing solution, projects, and `lib/three.js` submodule.
- Implement only the requested phase; do not anticipate later phases.
- Prefer real browser/WebView lifecycle validation over mock-only tests.
- Propagate cancellation and dispose JS resources deterministically.
- Update checklist items only after validation evidence exists.

## Completion report

Report changed files, commands/tests executed, validation results, remaining limitations, and checklist items intentionally left open.