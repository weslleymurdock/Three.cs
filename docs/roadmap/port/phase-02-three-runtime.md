# Port Phase 02 — Three.js Runtime

## Checklist

- [ ] Pin and document the upstream Three.js revision.
- [ ] Select the first supported Three.js modules and dependencies.
- [ ] Port/copy runtime assets with attribution and an update procedure.
- [ ] Define module loading and static asset paths.
- [ ] Define the JavaScript global/module boundary.
- [ ] Add runtime smoke tests.
- [ ] Validate loading in Blazor WASM.
- [ ] Document unsupported Three.js features.
- [ ] Record validation evidence and only then update the checklist.

## Exit criteria

The selected runtime loads from the RCL in a browser, its module boundary is stable, and automated smoke validation exists.