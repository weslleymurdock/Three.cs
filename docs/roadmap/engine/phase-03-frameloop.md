# Engine Phase 03 — Frame Loop

## Checklist

- [ ] Define monotonic elapsed/delta timing.
- [ ] Implement a cancellable deterministic frame loop.
- [ ] Avoid unbounded per-frame task creation.
- [ ] Make UI/thread affinity explicit at host boundaries.
- [ ] Separate simulation/update from render submission.
- [ ] Define frame exception handling and shutdown policy.
- [ ] Test cancellation, restart, timing, and exception behavior.
- [ ] Measure allocations and hot-path behavior where practical.

## Exit criteria

The engine owns one explicit frame loop that can start, stop, cancel, and restart without blocking or uncontrolled task creation.