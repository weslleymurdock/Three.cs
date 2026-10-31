# Port Phase 01 — Foundation

Phase 01 is structural, project-configuration and test-infrastructure work (Issue #4). It contains **no** Three.js runtime, bindings, JavaScript bridge, lifecycle, hosting/DI, or rendering implementation. Those belong to Port Phases 02–04 (Issues #5–#7). **Issue #4 defines the scope boundary; this checklist does not authorize earlier delivery of later-phase types.**

## Decisions on candidate types

Each type that earlier drafts associated with the Foundation was evaluated against Issue #4 ("no premature runtime/bindings/bridge implementation"; "do not implement future-phase functionality"), Issues #5/#6, and `AGENTS.md` ("no placeholder, speculative, or unused production types"). **None qualifies for Phase 01, so none is implemented.**

| Type | Belongs to | Decision and justification |
| --- | --- | --- |
| `ThreeLifecycleState`, `IThreeLifecycle`, `ThreeLifecycle` | Phase 03 (Issue #6: "typed C# object and lifecycle contracts", "deterministic disposal") | Not implemented. States and ownership rules only have meaning once JS-backed objects and a bridge exist. No consumer exists in Phase 01. |
| `JsInteropConstants` | Phase 02 (Issue #5: "JavaScript global/module boundary", "module loading and static asset paths") | Not implemented. The identifiers to centralize are decided by the runtime integration. Defining them now would be speculative. |
| `JsBridgeException`, `IJsBridge`, `JsBridge` | Phase 03 (Issue #6: "shared JS interop transport", "bridge readiness") | Not implemented. Transport, readiness and failure modes depend on the runtime and the MAUI bridge. |
| `ThreeOptions` | Phase 02/03 | Not implemented. It would be a speculative public configuration API with nothing to configure. |
| `ServiceCollectionExtensions` | Phase 03 (host composition) | Not implemented. No service exists to register, and registering nonexistent services is prohibited. |
| `IThreeRuntime`, `ThreeRuntime` | Phase 02 (Issue #5: runtime integration and initialization) | Not implemented. This is Three.js runtime behavior. |

Consequence: Phase 01 has **no executable production code**. The Foundation deliverable is the verified RCL/project configuration, namespace/folder/asset conventions, and the test infrastructure that enforces them.

## Checklist

- [x] Inspect the repository and identify template code without deleting project foundations.
  - **Details**: Reviewed `Three.cs.slnx`, `src/Three.cs/Three.cs.csproj`, `src/Three.Engine/Three.Engine.csproj`, `.config/dotnet-tools.json`, `.gitmodules` and the `lib/three.js` checkout. Template web-host artifacts in `Three.cs` (`Program.cs`, `appsettings*.json`, `Pages/`, `Properties/`, Bootstrap/jQuery `wwwroot` assets) and `Three.Engine/Class1.cs` are absent. The solution, both projects, the project reference and the submodule are preserved.
  - **Files**: `tests/Three.cs.Tests/TemplateRemovalTests.cs`, `tests/Three.cs.Tests/RepositoryInvariantTests.cs`, `tests/Three.cs.Tests/ProjectDependencyTests.cs`.
  - **Validation**: Evidence 3 (all tests pass).

- [x] Remove only obsolete template types/placeholders.
  - **Details**: No template artifact remains and no foundation project, reference or submodule was removed.
  - **Files**: `tests/Three.cs.Tests/TemplateRemovalTests.cs` (8 theory cases + 1 fact).
  - **Validation**: Evidence 3.

- [x] Keep `Three.cs` as the existing RCL and prepare it for browser and MAUI Hybrid consumption.
  - **Details**: `Three.cs.csproj` uses `Microsoft.NET.Sdk.Razor` (not `Microsoft.NET.Sdk.Web`), declares `SupportedPlatform Include="browser"`, sets `RootNamespace` to `Three`, and declares exactly one package, `Microsoft.AspNetCore.Components.Web` 10.0.14. It has no MAUI/platform dependency and no `ProjectReference`, so it does not depend on `Three.Engine`. The `.csproj` was not modified.
  - **Files**: `src/Three.cs/Three.cs.csproj`, `tests/Three.cs.Tests/ProjectConfigurationTests.cs` (including the new `ThreeCs_DeclaresOnlyTheWebComponentsPackage`), `tests/Three.cs.Tests/ProjectDependencyTests.cs` (including the new `ThreeCs_DoesNotReferenceThreeEngine`).
  - **Validation**: Evidence 3. **Not validated in this phase**: actual Blazor WASM or MAUI Hybrid hosting (Issues #5–#7).

- [x] Define supported .NET 10 target frameworks without introducing platform code into the RCL.
  - **Details**: `Three.cs`, `Three.Engine` and the test project target `net10.0` with nullable and implicit usings enabled. No MAUI or platform TFMs, packages or assembly references.
  - **Files**: `tests/Three.cs.Tests/ProjectConfigurationTests.cs`, `tests/Three.cs.Tests/PlatformIndependenceTests.cs`, `tests/Three.cs.Tests/AssemblyLoadingTests.cs`.
  - **Validation**: Evidence 3.

- [x] Establish static asset/module and JS interop folder conventions.
  - **Details**: `src/Three.cs/wwwroot/three/` and `src/Three.cs/wwwroot/js/` exist, each holding only `.gitkeep`. Three.js is neither ported nor bundled, and `lib/three.js` is untouched. A new boundary test fails if assets or scripts appear in these folders during Phase 01.
  - **Files**: `src/Three.cs/wwwroot/three/.gitkeep`, `src/Three.cs/wwwroot/js/.gitkeep`, `tests/Three.cs.Tests/StaticAssetOrganizationTests.cs`, `tests/Three.cs.Tests/PhaseBoundaryTests.cs`.
  - **Validation**: Evidence 3.

- [x] Establish namespace and directory conventions for bindings, runtime, interop, lifecycle, and hosting.
  - **Details**: `src/Three.cs/{Bindings,Hosting,Interop,Lifecycle,Runtime}/` exist under root namespace `Three`, giving the folder-derived namespaces `Three.Bindings`, `Three.Hosting`, `Three.Interop`, `Three.Lifecycle` and `Three.Runtime`. Each holds only `.gitkeep`. No type is declared, because every candidate belongs to a later phase (see decisions above). No namespace test is possible without types, so the convention is enforced through the directory layout and `RootNamespace`. `Three.cs` has no source files outside `wwwroot`, and `Three.Engine` has none at all.
  - **Files**: `tests/Three.cs.Tests/ArchitecturalDirectoryTests.cs`, `tests/Three.cs.Tests/PhaseBoundaryTests.cs`, `tests/Three.cs.Tests/ProjectConfigurationTests.cs` (`ThreeCs_ConfiguredWithRootNamespaceThree`).
  - **Validation**: Evidence 3.

- [x] Define the test strategy and first smoke-test location.
  - **Details**: `tests/Three.cs.Tests/` (xUnit, `net10.0`, `coverlet.collector`) references `Three.cs` and `Three.Engine` and is registered under `/tests/` in `Three.cs.slnx`. The suite covers assembly loading, solution registration, the `Three.Engine` → `Three.cs` project reference, .NET 10, nullable/implicit usings, RCL configuration, root namespace, platform independence, template removal, architecture directories, static assets, the Phase 01 boundary, and repository invariants (local tool manifest, submodule declaration/checkout, governance documents). One type per file.
  - **Files**: all files under `tests/Three.cs.Tests/`. New in this run: `PhaseBoundaryTests.cs`, `RepositoryInvariantTests.cs`, plus added cases in `ProjectConfigurationTests.cs` and `ProjectDependencyTests.cs`.
  - **Validation**: Evidence 3 (55 passed).

- [x] Apply `docs/GUIDELINES.md` to new code.
  - **Details**: The only code is test code: one type per file, nullable enabled, no blocking waits, no Git invocation. There is no production code, so async/cancellation, concurrency, interop, disposal and DI rules have nothing to apply to yet and are deferred to the phases that introduce those concerns.
  - **Validation**: Evidence 2 (Release build with 0 warnings and 0 errors).

- [x] Restore/build/test the solution successfully.
  - **Validation**: Evidence 2 and 3.

- [ ] Coverage quality gate for executable production code (≥ 90% line, method, and branch).
  - **Status**: **N/A, left unchecked and not claimed as passed.** `Three.cs` and `Three.Engine` contain no executable production code. Their built assemblies declare 0 types, so Cobertura lists both packages with 0 classes and ReportGenerator reports 0 coverable lines, 0 branches and 0 methods (Evidence 4). No code was added just to produce metrics. The gate applies per metric from the first phase that adds production code.

- [x] Record validation evidence before checking items.
  - **Details**: The evidence below comes from the commands listed.

## Validation Evidence

Environment: Windows, .NET SDK 10, repository-local tools, run on 2026-10-31.

### 1. Local tool
- `dotnet tool restore` → restored `dotnet-reportgenerator-globaltool` 5.5.12 from `.config/dotnet-tools.json`.
- The submodule revision pin (`cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168`) was **not** re-verified in this run, because verifying it needs a Git command and Git is out of scope for this task. The pin is not modified by any change here. `RepositoryInvariantTests` verifies only that `lib/three.js` is declared in `.gitmodules` and checked out (`package.json` present).

### 2. Restore and build
- `dotnet restore Three.cs.slnx` → all projects up to date.
- `dotnet build Three.cs.slnx --configuration Release` → succeeded, 0 warnings, 0 errors (`Three.cs`, `Three.Engine`, `Three.cs.Tests`).

### 3. Tests
- `dotnet test Three.cs.slnx --configuration Release --collect:"XPlat Code Coverage" --results-directory artifacts/test-results` → **55 passed, 0 failed, 0 skipped**.

### 4. Coverage (ReportGenerator)
- `dotnet tool run reportgenerator "-reports:artifacts/test-results/**/coverage.cobertura.xml" "-targetdir:artifacts/coverage" "-reporttypes:MarkdownSummary;TextSummary"` → wrote `artifacts/coverage/Summary.md` and `artifacts/coverage/Summary.txt`.
- Both report Assemblies 0, Classes 0, Files 0, Coverable lines 0, Total branches 0, Total methods 0 and "No assemblies have been covered". The Cobertura file lists `Three.cs` and `Three.Engine` with `<classes />`.
- **Measured coverage**: none. **N/A**: line, method and branch coverage, because there are no executable production types. **Failed gates**: none. The 90% thresholds were neither met nor claimed.
- Note: `Summary.md` prints method coverage as a sponsors-only ReportGenerator feature. `Summary.txt` is the authoritative source for method counts.

## Prepared for later phases

- **Phase 02 (Issue #5)**: `wwwroot/three/` and `wwwroot/js/` for runtime assets and modules, `Three.Runtime`/`Three.Interop` folders, and the `lib/three.js` submodule. Phase 02 defines the JS identifiers, module loading and runtime initialization (the former `JsInteropConstants` and `IThreeRuntime`/`ThreeRuntime` candidates).
- **Phase 03 (Issue #6)**: `Three.Bindings`, `Three.Lifecycle` and `Three.Hosting` folders. Phase 03 defines lifecycle contracts, the JS bridge and its failure type, DI registration and options (the former lifecycle, `IJsBridge`/`JsBridge`, `JsBridgeException`, `ThreeOptions` and `ServiceCollectionExtensions` candidates).
- **Phase 04 (Issue #7)**: cross-host validation (Blazor WASM, MAUI Hybrid) and packaging. Nothing is validated on those hosts yet.
- When a later phase intentionally adds content, it must update `PhaseBoundaryTests` and `ThreeCs_DeclaresOnlyTheWebComponentsPackage` in the same change and enable the coverage gate for the new code.

## Exit criteria

The existing solution, projects, project reference and submodule declaration are intact. Template placeholders are absent. The RCL and the directory/asset layout are ready for the runtime port. The structural test suite passes (55/55) with 0 build warnings. No runtime, bindings, bridge, lifecycle or hosting implementation exists, deliberately, per Issue #4. The coverage gate is N/A until executable production code exists, so the coverage item stays unchecked.
