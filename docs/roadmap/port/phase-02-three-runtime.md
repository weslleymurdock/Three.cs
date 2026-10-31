# Port Phase 02 — Three.js Runtime

Phase 02 integrates the pinned upstream Three.js JavaScript runtime into the `Three.cs` Razor Class Library Static Web Assets, establishes a host-independent JavaScript loading façade and minimal C# consumption API, and validates runtime loading and performance across Blazor WebAssembly and .NET MAUI Blazor Hybrid hosts (Issue #5).

## Checklist

- [x] Pin and document the upstream Three.js revision.
  - **Details**: Pinned submodule `lib/three.js` at revision `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168` (`r187-5-gcb0a902b66`, Three.js v0.187.0, MIT license). Full provenance documented in `docs/runtime/upstream.md`.
  - **Files**: `.gitmodules`, `src/Three.cs/wwwroot/three/upstream.json`, `docs/runtime/upstream.md`.
  - **Validation**: Evidence 1 & 3 (`ThreeRuntimeAssetIntegrityTests`).

- [x] Select the first supported Three.js modules and dependencies.
  - **Details**: Selected official ESM build files `build/three.module.js` and `build/three.core.js` from `lib/three.js`. Provides Core, Math, Scene Graph, Cameras, Geometries, Materials, Textures, Lights, and WebGLRenderer. Transitive dependency graph is self-contained (`three.module.js` -> `./three.core.js`). Excluded WebGPU, TSL, and CJS.
  - **Files**: `src/Three.cs/wwwroot/three/three.module.js`, `src/Three.cs/wwwroot/three/three.core.js`, `docs/runtime/upstream.md`.
  - **Validation**: Evidence 3 (`ThreeModule_OnlyImportsThreeCore`).

- [x] Port/copy runtime assets with attribution and an update procedure.
  - **Details**: Byte-for-byte copy from submodule with SHA-256 integrity verification. MIT license preserved. Created `scripts/Sync-ThreeRuntime.ps1` with `-Check` mode for automated drift detection.
  - **Files**: `src/Three.cs/wwwroot/three/LICENSE`, `scripts/Sync-ThreeRuntime.ps1`, `.gitattributes`.
  - **Validation**: Evidence 3 (`SyncScript_CheckMode_PassesWithoutDrift`).

- [x] Define module loading and static asset paths.
  - **Details**: Organized under `_content/Three.cs/three/` and `_content/Three.cs/js/`. Public specifier `./_content/Three.cs/js/three-runtime.js` resolves uniformly in Blazor WASM and MAUI BlazorWebView against document base URI.
  - **Files**: `src/Three.cs/Interop/ThreeRuntimeAssets.cs`, `src/Three.cs/wwwroot/js/three-runtime.js`, `docs/runtime/contract.md`.
  - **Validation**: Evidence 3 (`AssetConstants_MatchExpectedStaticAssetPaths`).

- [x] Define the JavaScript global/module boundary.
  - **Details**: Implemented `three-runtime.js` façade and `three-runtime-core.js` state machine. Exports contract version, `initialize()` (lazy, concurrent-safe, non-rejecting), `getState()`, `getUpstreamInfo()`, `getCapabilities()`, and `dispose()`. Minimal C# consumption surface delivered in `Three.Runtime` and `Three.Interop`.
  - **Files**: `src/Three.cs/wwwroot/js/three-runtime.js`, `src/Three.cs/wwwroot/js/three-runtime-core.js`, `src/Three.cs/Runtime/*`, `src/Three.cs/Interop/*`.
  - **Validation**: Evidence 2 (Node.js unit tests) & Evidence 3 (`ThreeRuntimeLifecycleTests`).

- [x] Add runtime smoke tests.
  - **Details**: Automated tests covering contract exports, module import, dependency resolution, capabilities, concurrency, error states, cancellation, idempotent disposal, and static web asset integrity.
  - **Files**: `tests/js/three-runtime.test.js`, `tests/Three.cs.Tests/ThreeRuntimeLifecycleTests.cs`, `tests/Three.cs.Tests/ThreeRuntimeAssetIntegrityTests.cs`, `tests/Three.cs.Tests/ThreeRuntimeModelTests.cs`, `tests/Three.cs.Tests/NodeJsRuntimeTests.cs`.
  - **Validation**: Evidence 2 & Evidence 3 (91 passed).

- [x] Validate loading in Blazor WASM.
  - **Details**: Created `tests/Three.cs.Validation.BlazorWasm`. Published and validated in a real Chromium browser via Playwright MCP (`http://localhost:8931/mcp`). Verified Static Web Asset loading, runtime initialization, upstream info, capabilities (`WebGL2: True`), disposal, repeat initialization, and error handling.
  - **Files**: `tests/Three.cs.Validation.BlazorWasm/*`, `scripts/Start-Mcp-Playwright.ps1`, `agents/mcp/antigravity.mcp.json`.
  - **Validation**: Evidence 4.

- [x] Validate loading in .NET MAUI Blazor Hybrid.
  - **Details**: Created `tests/Three.cs.Validation.Maui`. Validated in native WinUI 3 WebView2 runtime (Edge 156.0.0.0, Release x64). Collected performance metrics: cold startup, 5 warm iterations, JS performance.now() breakdown, and error counts. Documented environment status for Android (ready, requires elevated workload SDK), iOS, and MacCatalyst.
  - **Files**: `tests/Three.cs.Validation.Maui/*`, `artifacts/maui-validation-windows.json`.
  - **Validation**: Evidence 5.

- [x] Document unsupported Three.js features.
  - **Details**: Documented out-of-scope capabilities: WebGPU/TSL, CommonJS, and examples/jsm addons in `docs/runtime/upstream.md`.
  - **Files**: `docs/runtime/upstream.md`.
  - **Validation**: Documentation verified.

- [x] Record validation evidence and only then update the checklist.
  - **Details**: Evidence recorded below.

## Validation Evidence

Environment: Windows 11 (10.0.22631), .NET SDK 10.0.403, Node.js v24.22.1, Microsoft Edge WebView2 156.0.4314.50, Playwright MCP server at `http://localhost:8931/mcp`. Executed on 2026-10-31.

### 1. Submodule Integrity & Assets Sync
- `pwsh -NoProfile -File scripts/Sync-ThreeRuntime.ps1 -Check` -> **Succeeded (0 drift)**.
- Submodule `lib/three.js` verified at `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168` (`r187-5-gcb0a902b66`).
- SHA-256 integrity:
  - `three.module.js`: `52c8d1c5da42e31b77730009db4b41a9f353e908d9078fe57643e434631d3e0f`
  - `three.core.js`: `d3c91efb5164f9542c24a392c982e8168005c78f5189863cc8546fb0b831ecc1`
  - `LICENSE`: `5d5dad8147523d59bc15cca838ae0bdaae7075230ec49132c551a1d18e7b3412`

### 2. JavaScript Automated Unit Tests (Node.js)
Command: `node --test tests/js/three-runtime.test.js`
- **Result:** 6 passed, 0 failed, 0 skipped (duration: ~95 ms).
- Tested: real Three.js module import, capabilities, concurrent initialization serialization, import failure, incompatible exports verification, smoke test failure, and cancellation/disposal.

### 3. .NET Unit & Boundary Test Suite
Command:
```powershell
dotnet test Three.cs.slnx --configuration Release --logger "trx;LogFileName=test-results.trx" --collect:"XPlat Code Coverage" --results-directory artifacts/test-results
```
- **Result:** **91 passed, 0 failed, 0 skipped** (duration: ~2.5 s).
- Included tests: `ThreeRuntimeLifecycleTests` (20 tests covering `ImportAsync`, `InitializeAsync`, concurrency, idempotency, failure modes, cancellation, disposal), `ThreeRuntimeAssetIntegrityTests` (4 tests), `ThreeRuntimeModelTests` (8 tests), `NodeJsRuntimeTests` (1 test), `PhaseBoundaryTests` (3 tests), `StaticAssetOrganizationTests` (4 tests), `ArchitecturalDirectoryTests` (2 tests), `ProjectConfigurationTests` (5 tests), `ProjectDependencyTests` (2 tests), `PlatformIndependenceTests` (3 tests), `RepositoryInvariantTests` (3 tests), `TemplateRemovalTests` (9 tests), `AssemblyLoadingTests` (27 tests).

### 4. Code Coverage Quality Gate (ReportGenerator 5.5.12)
Command:
```powershell
dotnet tool run reportgenerator "-reports:artifacts/test-results/**/coverage.cobertura.xml" "-targetdir:artifacts/coverage" "-reporttypes:MarkdownSummary;TextSummary"
```
Report generated in `artifacts/coverage/Summary.txt`:
- **Line coverage:** **100%** (152 of 152 coverable lines) — **Gate Passed (≥ 90%)**
- **Method coverage:** **100%** (41 of 41 methods) — **Gate Passed (≥ 90%)**
- **Branch coverage:** **100%** (46 of 46 branches) — **Gate Passed (≥ 90%)**
- Individual breakdown for `Three.cs`:
  - `Three.Runtime.ThreeRuntime`: 100% line, 100% method, 100% branch
  - `Three.Runtime.ThreeUpstreamInfo`: 100% line, 100% method
  - `Three.Runtime.ThreeRuntimeCapabilities`: 100% line, 100% method
  - `Three.Runtime.ThreeRuntimeEnvironment`: 100% line, 100% method
  - `Three.Runtime.ThreeRuntimeTimings`: 100% line, 100% method
  - `Three.Runtime.ThreeRuntimeInitialization`: 100% line, 100% method
  - `Three.Runtime.ThreeRuntimeException`: 100% line, 100% method
  - `Three.Interop.ThreeRuntimeErrorInfo`: 100% line, 100% method
  - `Three.Interop.ThreeRuntimeInitializeResponse`: 100% line, 100% method

### 5. Blazor WebAssembly Integration Validation (Real Browser)
- Published `tests/Three.cs.Validation.BlazorWasm` to `artifacts/wasm-published`.
- Verified Static Web Assets placed in `_content/Three.cs/js/` and `_content/Three.cs/three/`.
- Navigated real browser via Playwright MCP (`http://localhost:8931/mcp`).
- Results verified on live browser DOM:
  - Runtime State: `Ready`
  - Upstream: `three` v0.187.0 r187 (loaded r187), commit `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168`, MIT
  - Capabilities verified: `core`, `scene-graph`, `cameras`, `geometries`, `materials`, `textures`, `lights`, `renderer-webgl`
  - WebGL2: `True`
  - Interop round trip: ~254 ms cold, ~17 ms warm
  - Lifecycle: `Dispose` transition to `Disposed`, subsequent `Repeat` re-initialized cleanly to `Ready`.

### 6. .NET MAUI Blazor Hybrid Validation (Native WebView)
- Project: `tests/Three.cs.Validation.Maui` (Release x64 unpackaged).
- Target executed: Windows (`net10.0-windows10.0.19041.0`).
- Environment: WinUI 3, OS 10.0.22631, Device: B660M DS3H DDR4.
- Native WebView: Microsoft Edge WebView2 (User Agent: `Chrome/156.0.0.0 Safari/537.36 Edg/156.0.0.0`).
- Metrics recorded in `artifacts/maui-validation-windows.json`:
  - **Native WebView Ready (`BlazorWebViewInitialized`):** 0.0015 ms
  - **Blazor Component Ready (`OnAfterRenderAsync(firstRender)`):** 284.68 ms
  - **Cold Module Import (C# `ImportAsync`):** 22.53 ms (JS `import()`: 50.90 ms)
  - **Cold Runtime Init (C# `InitializeAsync`):** 70.17 ms (JS verification: 6.70 ms)
  - **Cold Total Startup:** 92.70 ms (JS total: 57.60 ms)
  - **Warm Operations (Active Live Runtime, no artificial dispose):**
    - Mean: 2.37 ms, Median: 0.75 ms, Min: 0.05 ms, Max: 9.32 ms
    - Iteration 1 (Warm Re-Init Check): 0.05 ms
    - Iteration 2 (Warm Capabilities Query): 9.32 ms
    - Iteration 3 (Warm Upstream Metadata Query): 1.13 ms
    - Iteration 4 (Warm Capabilities Query): 0.75 ms
    - Iteration 5 (Warm Upstream Metadata Query): 0.58 ms
  - **Errors:** 0

### 7. Validation Limitations and Target Status
- **Windows (MAUI Hybrid):** Validated in native WinUI 3 WebView2 runtime (Release x64).
- **Blazor WebAssembly:** Validated in Chromium browser via Playwright MCP against published app.
- **Android (MAUI Hybrid):** NOT EXECUTED — environment unavailable. Host code is fully implemented and ready in `tests/Three.cs.Validation.Maui`. Building the Android target (`net10.0-android`) requires installing the .NET 10 Android AOT workload (`dotnet workload restore`), which failed due to missing administrative elevation for Windows MSI installer in `C:\Program Files\dotnet\`. No synthetic results declared.
- **iOS / MacCatalyst (MAUI Hybrid):** NOT EXECUTED — no Apple runtime/device environment available. Cross-compilation and execution for iOS and Mac Catalyst require macOS and Xcode, which are not available in this environment. The project target configuration is preserved.

### 8. Automated CI Workflow & Quality Gate
- GitHub Actions workflow implemented in `.github/workflows/build-tests.yml`.
- Workflow executes build, TRX test logger, Cobertura code coverage collection, ReportGenerator 5.5.12, and quality gate script `scripts/Evaluate-QualityGate.ps1`.
- Step summary publishes TRX results, Markdown summary, Text summary, and Quality Gate status.
- Quality Gate automatically fails the workflow (exit code 1) if line, method, or branch coverage drops below 90% or any test fails.

## Exit Criteria Verification

The selected Three.js runtime loads cleanly from the RCL Static Web Assets in both Blazor WebAssembly and .NET MAUI Blazor Hybrid WebViews, the module boundary is verified and stable, automated smoke and unit validation exists and passes (91/91 tests), and the code coverage gate passed at 100% across all individual metrics (line: 100%, method: 100%, branch: 100%).
