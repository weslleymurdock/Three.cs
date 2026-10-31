---
name: three-validation-agent
---
# Three.cs Validation Skill

Use this skill when a task claims a phase or feature is complete.

## Validation order

1. Inspect the diff and repository status.
2. Restore dependencies from a clean state where practical.
3. Build the relevant solution/projects.
4. Run unit/integration tests.
5. Exercise the actual browser or MAUI boundary when the feature crosses JS/WebView/graphics lifecycle.
6. Verify cancellation and disposal for long-lived runtime resources.
7. Record exact commands and outcomes.

A test that only verifies mocks or method invocation does not by itself prove a renderer, bridge, or runtime service works. Never mark a roadmap checkbox solely because code or unit tests exist.