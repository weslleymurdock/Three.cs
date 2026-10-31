# Agent Instructions

These instructions apply to every coding agent working in this repository. Read this file and `docs/GUIDELINES.md` before making changes. The task issue, its parent feature issue, and the applicable roadmap/checklist define the authorized scope.

## Repository invariants
- Preserve the existing `Three.cs.slnx`, `src/Three.cs/Three.cs.csproj`, `src/Three.Engine/Three.Engine.csproj`, their project reference, and the `lib/three.js` submodule.
- Do not change the pinned Three.js submodule revision unless the task explicitly requires it.
- Keep `Three.cs` portable and host-independent. Platform-specific code belongs in appropriate host projects, not the RCL core.
- Keep `Three.Engine` dependent on the public `Three.cs` surface, never on host implementation details.
- Implement only the task assigned. Do not start work from a later roadmap phase or broaden scope without an explicit dependency requiring it.

## Source organization and design
- Implement no more than one type per source file. Every class, record, struct, interface, enum, and delegate declaration must have its own file; do not group multiple types in one file, including helper or test types.
- Follow the namespace/folder conventions already established by the repository.
- Prefer clear, cohesive types and small focused files. Do not create placeholder, dummy, speculative, or unused production types.
- Follow `docs/GUIDELINES.md` for .NET 10, nullable references, async/cancellation, concurrency, interop, disposal, dependency injection, and architecture.
- Add XML documentation to public APIs where appropriate.

## Tests and quality gate
- Tests must exercise real behavior and meaningful user/developer journeys through the implementation.
- Use realistic cases, boundary conditions, state transitions, failure paths, cancellation, repeated lifecycle operations, and disposal scenarios where applicable.
- Use theories when multiple realistic input cases exercise the same behavior.
- Never add dummy tests, meaningless assertions, tests that merely instantiate a type, or tests whose only purpose is to inflate coverage.
- Never weaken, skip, delete, or distort a meaningful test to reach a coverage percentage.
- Coverage is a quality gate, not the objective. Achieve it through comprehensive tests of real behavior; if a meaningful path is difficult to test, improve the design/test seam rather than writing artificial tests.

### Required coverage workflow
The repository uses the local `.NET tool manifest` for ReportGenerator. Restore tools before running the report:
```powershell
dotnet tool restore
dotnet test Three.cs.slnx --collect:"XPlat Code Coverage" --results-directory artifacts/test-results
dotnet tool run reportgenerator "-reports:artifacts/test-results/**/coverage.cobertura.xml" "-targetdir:artifacts/coverage" "-reporttypes:MarkdownSummary;TextSummary"
```
ReportGenerator must produce both:
- `artifacts/coverage/Summary.md`
- `artifacts/coverage/Summary.txt`
Inspect both summaries. For every implementation, the minimum acceptable coverage is **90% for each of line coverage, method coverage, and branch coverage**. This is an individual threshold for all three metrics, not an average. A result below 90% in any metric means the implementation is not ready.
If the current test setup or ReportGenerator invocation produces no report, fix the workflow rather than claiming coverage. If a metric is not reported or cannot be measured, document the limitation and do not claim that the gate passed. Do not commit generated coverage artifacts unless the task explicitly asks for them.

## Validation and evidence
- Restore, build, and run the relevant tests; execute the full solution test suite when feasible.
- Run the coverage workflow above and record the measured line, method, and branch percentages.
- Validate behavior, not just compilation or mocked interactions. Use integration/host tests when the task requires them and the environment supports them.
- Never claim a platform, scenario, test, or coverage threshold was validated unless it actually ran and its output was inspected.
- Record exact commands, results, limitations, and environment constraints in the task checklist or implementation report.
- Update roadmap checkboxes only after the corresponding implementation and validation are complete and evidenced. Leave incomplete items unchecked.
- Do not report a task as complete if any acceptance criterion or required quality gate remains unmet.

## Git workflow
- Keep commits focused and use semantic commit messages.
- Do not rewrite shared history or force-push unless explicitly authorized.
- Do not include unrelated generated files, local settings, secrets, or build artifacts in commits.
