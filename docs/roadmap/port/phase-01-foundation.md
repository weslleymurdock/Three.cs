# Port Phase 01 — Foundation

## Checklist

- [ ] Inspect the complete repository and identify template code without deleting project foundations.
- [ ] Remove only obsolete template types/placeholders from `Three.cs`.
- [ ] Keep `Three.cs` as the existing RCL and prepare it for browser and MAUI Hybrid consumption.
- [ ] Define supported .NET 10 target frameworks without introducing platform code into the RCL.
- [ ] Establish static asset/module and JS interop folder conventions.
- [ ] Establish namespace conventions for bindings, runtime, interop, and lifecycle.
- [ ] Define the test strategy and first smoke-test location.
- [ ] Apply `docs/GUIDELINES.md` to new code.
- [ ] Restore/build/test the solution successfully.
- [ ] Record validation evidence before checking items.

## Exit criteria

The existing solution and submodule remain intact, template placeholders are removed, the project structure is ready for the runtime port, and the solution validates cleanly.