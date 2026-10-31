# Port Phase 01 — Foundation

Phase 01 is structural and project-configuration work only. It contains **no** Three.js runtime, bindings, JavaScript bridge, lifecycle, hosting/DI, or rendering implementation. Those belong to Port Phases 02–04 (Issues #5–#7).

## Checklist

- [x] Inspect the complete repository and identify template code without deleting project foundations.
  - **Details**: Inspected `Three.cs.slnx`, `src/Three.cs/Three.cs.csproj`, `src/Three.Engine/Three.Engine.csproj`, and the `lib/three.js` submodule. Identified ASP.NET Core web host template boilerplate in `Three.cs` (`Program.cs`, `appsettings*.json`, `Pages/`, `Properties/`, template `wwwroot/` assets) and the `Class1.cs` placeholder in `Three.Engine`.
  - **Validation**: Recorded in repository history; solution, both projects, project reference, and submodule are preserved (see evidence 1 and 3).

- [x] Remove only obsolete template types/placeholders from `Three.cs`.
  - **Details**: Obsolete web host files, Bootstrap/jQuery `wwwroot` assets, and the `Three.Engine` `Class1.cs` placeholder are absent. No foundation project, reference, or submodule was removed.
  - **Validation**: `TemplateRemovalTests` (8 theory cases + 1 fact) asserts these artifacts do not exist.

- [x] Keep `Three.cs` as the existing RCL and prepare it for browser and MAUI Hybrid consumption.
  - **Details**: `Three.cs.csproj` uses `Microsoft.NET.Sdk.Razor` (not `Microsoft.NET.Sdk.Web`), declares `SupportedPlatform Include="browser"`, references `Microsoft.AspNetCore.Components.Web` 10.0.14, and sets `RootNamespace` to `Three`.
  - **Validation**: `ProjectConfigurationTests` (`ThreeCs_ConfiguredWithRazorSdkAndWebComponents`, `ThreeCs_ConfiguredWithRootNamespaceThree`). Browser/MAUI Hybrid *hosting* is not validated in this phase.

- [x] Define supported .NET 10 target frameworks without introducing platform code into the RCL.
  - **Details**: `Three.cs` and `Three.Engine` target the portable `net10.0` TFM with nullable and implicit usings enabled. No MAUI or platform packages/TFMs are referenced.
  - **Validation**: `ProjectConfigurationTests` (`Projects_TargetDotNet10`, `Projects_HaveNullableAndImplicitUsingsEnabled`), `PlatformIndependenceTests`, and `AssemblyLoadingTests.Assembly_TargetsExpectedFramework`.

- [x] Establish static asset/module and JS interop folder conventions.
  - **Details**: `src/Three.cs/wwwroot/three/` and `src/Three.cs/wwwroot/js/` exist as empty, `.gitkeep`-tracked locations for future runtime assets and interop scripts. Three.js is neither ported nor bundled. The `lib/three.js` submodule is untouched.
  - **Validation**: `StaticAssetOrganizationTests`.

- [x] Establish namespace and directory conventions for bindings, runtime, interop, lifecycle, and hosting.
  - **Details**: Structural preparation only. `src/Three.cs/{Bindings,Runtime,Interop,Lifecycle,Hosting}/` exist as empty, `.gitkeep`-tracked directories under root namespace `Three` (folder-derived namespaces, e.g. `Three.Interop`). No types, contracts, or implementations are placed in them; they are populated by later phases.
  - **Validation**: `ArchitecturalDirectoryTests` (directory + `.gitkeep` for all five), `ProjectConfigurationTests.ThreeCs_ConfiguredWithRootNamespaceThree`.

- [x] Define the test strategy and first smoke-test location.
  - **Details**: `tests/Three.cs.Tests/` (xUnit, `net10.0`) references `Three.cs` and `Three.Engine` and is registered under `/tests/` in `Three.cs.slnx`. The suite validates structure and project configuration only: assembly loading, solution registration, project reference, .NET 10, nullable, RCL configuration, root namespace, platform independence, template removal, architectural directories, and static asset directories. One type per file.
  - **Validation**: `dotnet test` result in evidence 3; `ProjectDependencyTests` validates solution registration and the `Three.Engine` → `Three.cs` project reference.

- [x] Apply `docs/GUIDELINES.md` to new code.
  - **Details**: The only new C# code is test code, one type per file, nullable enabled. There is no production code, so async/cancellation, concurrency, interop, disposal, and DI guidelines have nothing to apply to yet and are intentionally deferred to the phases that introduce those concerns.
  - **Validation**: Release build with 0 warnings and 0 errors (evidence 2); source review of the final diff.

- [x] Restore/build/test the solution successfully.
  - **Validation**: Evidence 2 and 3 below.

- [ ] Coverage quality gate for executable production code (≥ 90% line, method, and branch).
  - **Status**: **Not applicable for Phase 01 and not claimed as passed.** `Three.cs` and `Three.Engine` contain no executable production code, so ReportGenerator reports 0 coverable lines, 0 branches, and 0 methods (see evidence 4). The gate applies, per metric, to the first phase that adds executable production code.

- [x] Record validation evidence before checking items.
  - **Details**: Evidence below was produced by executing the listed commands.

## Validation Evidence

Environment: Windows, .NET SDK 10, repository-local tools.

### 1. Local tool and submodule
- `dotnet tool restore` → restored `dotnet-reportgenerator-globaltool` 5.5.12.
- `dotnet tool list --local` → `dotnet-reportgenerator-globaltool 5.5.12 reportgenerator` from `.config/dotnet-tools.json` (the manifest is the source of truth; version 5.5.12).
- `git submodule status` → `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168 lib/three.js (r187-5-gcb0a902b66)` (unchanged).

### 2. Restore and build
- `dotnet restore Three.cs.slnx` → all projects up to date.
- `dotnet build Three.cs.slnx --configuration Release` → succeeded, 0 warnings, 0 errors (`Three.cs`, `Three.Engine`, `Three.cs.Tests`).

### 3. Tests
- `dotnet test Three.cs.slnx --configuration Release --collect:"XPlat Code Coverage" --results-directory artifacts/test-results` → 39 passed, 0 failed, 0 skipped.

### 4. Coverage (ReportGenerator)
- `dotnet tool run reportgenerator "-reports:artifacts/test-results/**/coverage.cobertura.xml" "-targetdir:artifacts/coverage" "-reporttypes:MarkdownSummary;TextSummary"` → produced `artifacts/coverage/Summary.md` and `artifacts/coverage/Summary.txt`.
- Both summaries report: Assemblies 0, Classes 0, Files 0, Coverable lines 0, Total branches 0, Total methods 0, and "No assemblies have been covered." Line, branch, and method percentages are empty because there is nothing to measure.
- **Measured coverage**: none. **Not applicable**: line, method, and branch coverage (no executable production code). **Failed gates**: none. The 90% thresholds were neither met nor claimed.
- Note: `Summary.md` prints method coverage as a sponsors-only ReportGenerator feature; `Summary.txt` is the authoritative method-count source.

## Exit criteria

The existing solution, projects, project reference, and submodule remain intact; template placeholders are removed; the RCL and directory/asset structure are ready for the runtime port; and the structural test suite passes (39/39) with 0 build warnings. No runtime, bindings, bridge, lifecycle, or hosting implementation exists in this phase. The coverage gate is not applicable until executable production code exists.
