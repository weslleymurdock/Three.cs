# Three.js Runtime Loading Contract

## Overview
The Three.cs runtime delivers a host-independent JavaScript entry point that loads, verifies, and provides diagnostics for Three.js in both Blazor WebAssembly and .NET MAUI Blazor Hybrid hosts.

## Static Web Assets Layout
Assets are packaged inside the Razor Class Library `Three.cs` and served under the standard ASP.NET Core Static Web Assets prefix:

```text
_content/Three.cs/
  ├── js/
  │    ├── three-runtime.js        # Public stable entry point (façade)
  │    ├── three-runtime-core.js   # Lifecycle state machine & capability probe
  │    └── three-upstream.js       # Generated provenance constants
  └── three/
       ├── three.module.js         # Upstream ESM entry point
       ├── three.core.js           # Upstream core modules
       ├── LICENSE                 # Upstream MIT license
       └── upstream.json           # Integrity and provenance metadata
```

Hosts reference the module using the stable specifier:
`./_content/Three.cs/js/three-runtime.js`
This resolves correctly against the document base URI in both Blazor WASM and MAUI BlazorWebView.

## JavaScript API Contract (`three-runtime.js`)

| Export | Type | Description |
| --- | --- | --- |
| `contractVersion` | `number` | Version of the JavaScript runtime contract (currently `1`). |
| `initialize()` | `() => Promise<InitializationResult>` | Lazily imports and verifies Three.js. Concurrent calls share one initialization. Never rejects: errors are returned in `result.error`. |
| `getState()` | `() => RuntimeStateInfo` | Returns current state (`uninitialized`, `initializing`, `ready`, `failed`, `disposed`), timings, and last error. |
| `getUpstreamInfo()` | `() => UpstreamInfo` | Returns provenance metadata including version and loaded `REVISION`. |
| `getCapabilities()` | `() => CapabilitiesInfo` | Returns available capability groups and WebGL2 environment support. Throws `ThreeRuntimeError` if not ready. |
| `dispose()` | `() => void` | Releases runtime references. Idempotent. |

### InitializationResult Shape
```json
{
  "ready": true,
  "status": "ready",
  "upstream": {
    "name": "three",
    "version": "0.187.0",
    "revision": "187",
    "commit": "cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168",
    "license": "MIT",
    "repository": "https://github.com/mrdoob/three.js",
    "loadedRevision": "187"
  },
  "capabilities": {
    "available": ["core", "scene-graph", "cameras", "geometries", "materials", "textures", "lights", "renderer-webgl"],
    "unavailable": [],
    "environment": {
      "webgl2": true
    }
  },
  "timings": {
    "importMs": 64.0,
    "initializeMs": 7.9,
    "totalMs": 71.9
  },
  "error": null
}
```

## Error Codes and Handling

| Error Code | Stage | Cause |
| --- | --- | --- |
| `import-failed` | `import` | Network, 404, or asset resolution failure fetching `three.module.js` or `three.core.js`. |
| `incompatible-runtime` | `verify` | Module loaded but missing required core symbols (`REVISION`, `Scene`, `Mesh`, etc.). |
| `initialization-failed` | `initialize` | Post-load smoke verification failed (e.g. vector math or scene hierarchy abnormality). |
| `module-import-failed` | `import` | C# JS interop failure resolving the entry point specifier. |
| `js-error` | `initialize` or `query` | Unhandled JavaScript exception during execution. |
| `invalid-response` | `initialize` | Malformed payload returned to C#. |
| `not-initialized` | `query` | Query attempted while runtime was not ready. |
| `disposed` | `initialize` or `query` | Operation attempted on disposed runtime or disposed during in-flight initialization. |

## C# Integration (`Three.Runtime`)
- `IThreeRuntime`: interface exposing `State`, `InitializeAsync`, `GetUpstreamInfoAsync`, `GetCapabilitiesAsync`, and `IAsyncDisposable`.
- `ThreeRuntime`: thread-safe implementation using an asynchronous gate (`SemaphoreSlim`), propagating `CancellationToken`, providing idempotent disposal, and converting JS errors into typed `ThreeRuntimeException`.

