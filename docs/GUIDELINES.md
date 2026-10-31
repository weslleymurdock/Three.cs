# Engineering Guidelines

## Scope

These rules apply to production code, tests, and agent-generated changes throughout Three.cs.

## .NET 10 and source organization
- Enable nullable reference types and use modern C# features where they improve clarity.
- **Implement exactly one type per source file.** Each class, record, struct, interface, enum, or delegate must be declared in its own file. Do not place multiple types in a single file, including helper, nested-independent, or test types.
- Public APIs require XML documentation when appropriate.
- Keep portable core APIs independent of MAUI platform types.
- Prefer primary constructors, collection expressions, and async APIs when they improve the design rather than for stylistic novelty.

## Async and cancellation
- Never use `.Result`, `.Wait()`, or `GetAwaiter().GetResult()` in library/runtime code.
- Propagate `CancellationToken` through asynchronous boundaries.
- Use `Task`/`ValueTask` deliberately; do not create asynchronous work solely to satisfy an API shape.
- Avoid `async void` except framework-required event handlers.
- Never hide cancellation as a generic failure.

## Concurrency and frame execution
- Never create an unbounded `Task` per frame, render call, or input event.
- Prefer a single explicit frame-loop owner, bounded channels, queues, or long-lived loops.
- Keep simulation/update separate from render submission.
- Do not hold locks across `await`.
- Use immutable snapshots or message passing where they simplify synchronization.
- Use `Task.Run` only for genuinely CPU-bound work that benefits from a worker thread.

## JS/WebView interop
- Centralize JS identifiers and interop contracts.
- Require explicit bridge readiness before invocation.
- Make bridge injection idempotent and tied to WebView lifecycle.
- Never retain JavaScript references after their C# owner is disposed.
- Convert bridge failures into actionable diagnostics.

## Disposal and ownership
- Implement `IDisposable`/`IAsyncDisposable` when the type owns external resources.
- Disposal must be idempotent.
- Dispose dependencies in reverse ownership order.
- Cooperate with cancellation and shutdown.
- Do not silently swallow disposal failures without a deliberate policy.

## DI and architecture
- Registration belongs in extension methods; composition belongs at the host boundary.
- Keep lifetimes explicit.
- Do not put platform-specific services in portable core projects.
- `Three.Engine` depends on `Three.cs` abstractions and never reaches into host implementation details.

## Testing and coverage
- Prefer tests that exercise real lifecycle boundaries over tests that only assert mocks were called.
- Use realistic test cases, theories, and end-to-end journeys through meaningful implementation behavior.
- Never add dummy tests or tests whose only purpose is to increase coverage.
- Coverage is a quality gate, not a target to game: line, method, and branch coverage must each be at least **90%** for every implementation.
- Use the repository-local ReportGenerator .NET tool. Run `dotnet tool restore`, collect Cobertura coverage with `dotnet test`, then generate both `Summary.md` and `Summary.txt` with ReportGenerator. Inspect both reports and record the three measured percentages.
- A roadmap checkbox may only be marked complete after the implementation, tests, and required coverage validation have actually run and the evidence has been recorded.
- Do not weaken or fabricate tests to meet a percentage. Improve testability and cover real behavior, edge cases, lifecycle transitions, and failure paths.

## Git and submodules
- Keep commits focused and semantic.
- Do not change `lib/three.js` submodule revision unless the task explicitly requires an upstream revision change.
- Never recreate or replace the existing solution/projects merely to make a task easier.
