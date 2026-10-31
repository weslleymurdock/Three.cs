# Engine Phase 04 — Runtime

## Checklist

- [ ] Define scene/entity ownership boundaries.
- [ ] Add resource caches/registries where justified.
- [ ] Define platform-independent input contracts.
- [ ] Implement start/pause/resume/stop lifecycle semantics.
- [ ] Integrate update and render phases.
- [ ] Dispose resources in reverse dependency order.
- [ ] Test lifecycle and resource cleanup.
- [ ] Validate repeated start/stop/dispose safety.

## Exit criteria

The runtime integrates scene, resources, input, update, render, and lifecycle semantics without leaking host-specific concerns into the core.