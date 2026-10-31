# Engineering Guidelines

## Scope

These rules apply to production code and agent-generated changes in Three.cs.

## .NET 10

- Enable nullable reference types and use modern C# features where they improve clarity.
- Prefer one public type per file.
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

## Testing

- Prefer tests that exercise real lifecycle boundaries over tests that only assert mocks were called.
- Validate initialization, repeated start/stop, cancellation, disposal, and failure paths.
- A roadmap checkbox may only be marked complete after the implementation and its validation have actually run.

## Git and submodules

- Keep commits focused and semantic.
- Do not change `lib/three.js` submodule revision unless the task explicitly requires an upstream revision change.
- Never recreate or replace the existing solution/projects merely to make a task easier.