# Port Phase 01 — Foundation

## Checklist

- [x] Inspect the complete repository and identify template code without deleting project foundations.
  - **Details**: Inspected `Three.cs.slnx`, `src/Three.cs/Three.cs.csproj`, `src/Three.Engine/Three.Engine.csproj`, and git submodule `lib/three.js`. Identified ASP.NET Core webapp template boilerplate in `Three.cs` (`Program.cs`, `appsettings*.json`, `Pages/`, `Properties/`, template `wwwroot/`) and placeholder `Class1.cs` in `Three.Engine`.
  - **Files**: `Three.cs.slnx`, `src/Three.cs/*`, `src/Three.Engine/*`, `lib/three.js`.
  - **Validation**: Verified repository structure, project configurations, and preserved project files.

- [x] Remove only obsolete template types/placeholders from `Three.cs`.
  - **Details**: Removed obsolete ASP.NET Core web host files (`Program.cs`, `appsettings.json`, `appsettings.Development.json`, `Properties/launchSettings.json`, `Pages/`), Bootstrap/jQuery assets from `wwwroot/`, and `Class1.cs` placeholder from `Three.Engine`.
  - **Files removed**: `src/Three.cs/Program.cs`, `src/Three.cs/Pages/*`, `src/Three.cs/Properties/*`, `src/Three.cs/wwwroot/{css,js,lib,favicon.ico}`, `src/Three.Engine/Class1.cs`.
  - **Validation**: Git history tracks clean deletions, no functional or architecture types were removed.

- [x] Keep `Three.cs` as the existing RCL and prepare it for browser and MAUI Hybrid consumption.
  - **Details**: Converted `Three.cs.csproj` from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk.Razor`, added `SupportedPlatform Include="browser"`, and referenced `Microsoft.AspNetCore.Components.Web` (10.0.14). Configured `RootNamespace` as `Three`.
  - **Files**: `src/Three.cs/Three.cs.csproj`.
  - **Validation**: Verified compilation as a class library consumable by Blazor WASM and MAUI Hybrid targets.

- [x] Define supported .NET 10 target frameworks without introducing platform code into the RCL.
  - **Details**: Pinned portable `net10.0` target framework on `Three.cs` and `Three.Engine`. No MAUI platform-specific packages or platform TFMs were introduced into core libraries, preserving host independence.
  - **Files**: `src/Three.cs/Three.cs.csproj`, `src/Three.Engine/Three.Engine.csproj`.
  - **Validation**: `PlatformIndependenceTests` verifies absence of platform dependencies in both compiled assemblies and project configurations.

- [x] Establish static asset/module and JS interop folder conventions.
  - **Details**: Created `src/Three.cs/wwwroot/three/` (for future Three.js modules/bundles) and `src/Three.cs/wwwroot/js/` (for future bridge/interop code), preserved structurally with `.gitkeep`. Submodule `lib/three.js` is preserved untouched.
  - **Files**: `src/Three.cs/wwwroot/three/.gitkeep`, `src/Three.cs/wwwroot/js/.gitkeep`.
  - **Validation**: `StaticAssetOrganizationTests` validates directory existence and Git persistence markers.

- [x] Establish namespace conventions for bindings, runtime, interop, and lifecycle.
  - **Details**: Set root namespace to `Three` and established architectural directories with foundation contracts and implementations: `Lifecycle/` (`ThreeLifecycleState`, `IThreeLifecycle`, `ThreeLifecycle`), `Interop/` (`JsInteropConstants`, `JsBridgeException`, `IJsBridge`, `JsBridge`), `Hosting/` (`ThreeOptions`, `ServiceCollectionExtensions`), and `Runtime/` (`IThreeRuntime`, `ThreeRuntime`).
  - **Files**: `src/Three.cs/Lifecycle/*`, `src/Three.cs/Interop/*`, `src/Three.cs/Hosting/*`, `src/Three.cs/Runtime/*`.
  - **Validation**: `ArchitecturalDirectoryTests`, `ProjectConfigurationTests`, and domain unit tests validate architecture, conventions, and runtime behavior.

- [x] Define the test strategy and first smoke-test location.
  - **Details**: Created test suite project `tests/Three.cs.Tests/` using xUnit and .NET 10, referenced both `Three.cs` and `Three.Engine`, and registered the project under `/tests/` in `Three.cs.slnx`. Implemented modular test architecture with exactly one type per file covering assembly loading, dependencies, platform independence, static assets, directory structure, configuration, lifecycle, JS bridge, options, and DI registration.
  - **Files**: `tests/Three.cs.Tests/Three.cs.Tests.csproj`, `tests/Three.cs.Tests/*`, `Three.cs.slnx`.
  - **Validation**: `dotnet test Three.cs.slnx` runs and passes 102/102 tests.

- [x] Apply `docs/GUIDELINES.md` to new code.
  - **Details**: Ensured .NET 10 SDK compatibility, nullable reference types enabled across all projects, one type per source file strictly enforced, host independence verified, determinism maintained, no blocking waits (`.Result`, `.Wait()`), explicit ownership and disposal, and deliberate DI extensions.
  - **Files**: All modified/created project and source files.
  - **Validation**: Solution compiles cleanly with zero warnings and zero errors.

- [x] Restore/build/test the solution successfully.
  - **Details**: Executed solution restore, build, and test suite with cross-platform code coverage collection. Verified `Three.Engine -> Three.cs` project reference works and assemblies load correctly.
  - **Commands**:
    - `dotnet restore Three.cs.slnx` -> All projects up to date.
    - `dotnet build Three.cs.slnx --configuration Release` -> 0 Warnings, 0 Errors.
    - `dotnet test Three.cs.slnx --configuration Release --collect:"XPlat Code Coverage" --results-directory artifacts/test-results` -> 102 Passed, 0 Failed, 0 Skipped.
  - **Validation**: Automated CLI outputs recorded.

- [x] Record validation evidence before checking items.
  - **Details**: Validation evidence recorded below.

## Validation Evidence

### 1. Git Submodule Check
- **Command**: `git submodule status`
- **Output**: `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168 lib/three.js (r187-5-gcb0a902b66)`
- **Status**: Submodule remains pinned at target commit without modifications.

### 2. Solution Build
- **Command**: `dotnet build Three.cs.slnx --configuration Release`
- **Result**:
  - `Three.cs -> src\Three.cs\bin\Release\net10.0\Three.cs.dll`
  - `Three.Engine -> src\Three.Engine\bin\Release\net10.0\Three.Engine.dll`
  - `Three.cs.Tests -> tests\Three.cs.Tests\bin\Release\net10.0\Three.cs.Tests.dll`
  - Build succeeded with 0 Warnings, 0 Errors.

### 3. Test Suite Execution
- **Command**: `dotnet test Three.cs.slnx --configuration Release --collect:"XPlat Code Coverage" --results-directory artifacts/test-results`
- **Result**:
  - Test run completed with 102 tests passed, 0 failed, 0 skipped.
  - Validates assembly loading, cross-project references, platform independence, architectural directory layout, static asset folder conventions, lifecycle transitions, JS interop bridge, hosting options, and dependency injection registration.

### 4. Code Coverage Quality Gate (ReportGenerator)
- **Tool Version**: `dotnet-reportgenerator-globaltool` 5.5.12
- **Command**: `dotnet tool run reportgenerator "-reports:artifacts/test-results/**/coverage.cobertura.xml" "-targetdir:artifacts/coverage" "-reporttypes:MarkdownSummary;TextSummary"`
- **Output Locations**:
  - `artifacts/coverage/Summary.md`
  - `artifacts/coverage/Summary.txt`
- **Measured Metrics**:
  - **Line coverage**: 98.3% (179 of 182 lines) (Threshold: >= 90.0% — PASSED)
  - **Method coverage**: 100.0% (29 of 29 methods) (Threshold: >= 90.0% — PASSED)
  - **Branch coverage**: 91.0% (71 of 78 branches) (Threshold: >= 90.0% — PASSED)

## Exit criteria

The existing solution and submodule remain intact, template placeholders are removed, foundation contracts and types are implemented with strict one-type-per-file compliance, the project structure is ready for the runtime port, and all quality gates (0 warnings, 102/102 tests passing, and >=90% line, method, and branch coverage) pass cleanly.
