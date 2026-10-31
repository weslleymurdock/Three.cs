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
  - **Validation**: `Assemblies_ArePlatformIndependent` test verifies absence of platform dependencies.

- [x] Establish static asset/module and JS interop folder conventions.
  - **Details**: Created `src/Three.cs/wwwroot/three/` (for future Three.js modules/bundles) and `src/Three.cs/wwwroot/js/` (for future bridge/interop code), preserved structurally with `.gitkeep`. Submodule `lib/three.js` is preserved untouched.
  - **Files**: `src/Three.cs/wwwroot/three/.gitkeep`, `src/Three.cs/wwwroot/js/.gitkeep`.
  - **Validation**: `ThreeCs_StaticAssetsStructure_ContainsExpectedDirectories` smoke test passes.

- [x] Establish namespace conventions for bindings, runtime, interop, and lifecycle.
  - **Details**: Set root namespace to `Three` and established architectural directories: `Bindings/`, `Interop/`, `Runtime/`, `Lifecycle/`, and `Hosting/` (for Blazor WASM and MAUI WebView bridge integration).
  - **Files**: `src/Three.cs/{Bindings,Hosting,Interop,Lifecycle,Runtime}/.gitkeep`.
  - **Validation**: `ThreeCs_ContainsExpectedArchitectureDirectories` smoke test passes.

- [x] Define the test strategy and first smoke-test location.
  - **Details**: Created test suite project `tests/Three.cs.Tests/` using xUnit and .NET 10, referenced both `Three.cs` and `Three.Engine`, and registered the project under `/tests/` in `Three.cs.slnx`. Implemented foundational smoke and architecture tests.
  - **Files**: `tests/Three.cs.Tests/Three.cs.Tests.csproj`, `tests/Three.cs.Tests/FoundationSmokeTests.cs`, `Three.cs.slnx`.
  - **Validation**: `dotnet test Three.cs.slnx` runs and passes 14/14 tests.

- [x] Apply `docs/GUIDELINES.md` to new code.
  - **Details**: Ensured .NET 10 SDK compatibility, nullable reference types enabled across all projects, host independence verified, determinism maintained, no blocking waits or dummy classes created.
  - **Files**: All modified/created project and source files.
  - **Validation**: Solution compiles cleanly with zero warnings and zero errors.

- [x] Restore/build/test the solution successfully.
  - **Details**: Executed solution restore, build, and test suite. Verified `Three.Engine -> Three.cs` project reference works and assemblies load correctly.
  - **Commands**:
    - `dotnet build Three.cs.slnx` -> 0 Warnings, 0 Errors.
    - `dotnet test Three.cs.slnx` -> 14 Passed, 0 Failed, 0 Skipped.
  - **Validation**: Automated CLI outputs recorded.

- [x] Record validation evidence before checking items.
  - **Details**: Validation evidence recorded below.

## Validation Evidence

### 1. Git Submodule Check
- **Command**: `git submodule status`
- **Output**: `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168 lib/three.js (r187-5-gcb0a902b66)`
- **Status**: Submodule remains pinned at target commit without modifications.

### 2. Solution Build
- **Command**: `dotnet build Three.cs.slnx`
- **Result**:
  - `Three.cs -> src\Three.cs\bin\Debug\net10.0\Three.cs.dll`
  - `Three.Engine -> src\Three.Engine\bin\Debug\net10.0\Three.Engine.dll`
  - `Three.cs.Tests -> tests\Three.cs.Tests\bin\Debug\net10.0\Three.cs.Tests.dll`
  - Build succeeded with 0 Warnings, 0 Errors.

### 3. Test Suite Execution
- **Command**: `dotnet test Three.cs.slnx`
- **Result**:
  - Test run completed with 14 tests passed, 0 failed, 0 skipped.
  - Validates assembly loading, cross-project references, platform independence, architectural directory layout, and static asset folder conventions.

## Exit criteria

The existing solution and submodule remain intact, template placeholders are removed, the project structure is ready for the runtime port, and the solution validates cleanly.
