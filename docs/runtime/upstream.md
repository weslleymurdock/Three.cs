# Three.js Upstream Runtime Integration

## Provenance and Pinned Revision
- **Submodule Path:** `lib/three.js`
- **Pinned Commit:** `cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168`
- **Git Describe:** `r187-5-gcb0a902b66`
- **Upstream Release Version:** `0.187.0` (Revision `187`)
- **License:** MIT
- **Repository:** [https://github.com/mrdoob/three.js](https://github.com/mrdoob/three.js)

## Selected Runtime Modules and Dependencies
The canonical source for the runtime build is the official upstream ECMAScript Module (ESM) output located in `lib/three.js/build/`:

| Shipped File | Upstream Source | Size (Bytes) | SHA-256 Hash | Purpose |
| --- | --- | --- | --- | --- |
| `wwwroot/three/three.module.js` | `build/three.module.js` | 678,437 | `52c8d1c5da42e31b77730009db4b41a9f353e908d9078fe57643e434631d3e0f` | Main ESM entry point: WebGLRenderer, WebGLRenderTarget, shaders, and re-export of core. |
| `wwwroot/three/three.core.js` | `build/three.core.js` | 1,535,961 | `d3c91efb5164f9542c24a392c982e8168005c78f5189863cc8546fb0b831ecc1` | Core scene graph, math, cameras, geometries, materials, textures, loaders, and lights. |
| `wwwroot/three/LICENSE` | `LICENSE` | 1,102 | `5d5dad8147523d59bc15cca838ae0bdaae7075230ec49132c551a1d18e7b3412` | Upstream MIT License and copyright notice. |
| `wwwroot/three/upstream.json` | Generated | 778 | Verified | Machine-readable provenance manifest. |

### Transitive Dependencies
`three.module.js` has exactly one static relative dependency: `./three.core.js`. There are no external npm packages or CDN dependencies.

### Technical Justification for Selected Modules
- `three.core.js` and `three.module.js` provide the complete foundation for 3D applications (Math, Object3D, Scene, Camera, Geometry, Material, Texture, Light, WebGLRenderer) without bundling experimental or out-of-scope subsystems.
- Excluded modules: WebGPU (`three.webgpu.js`, `three.webgpu.nodes.js`) and TSL (`three.tsl.js`) are out of scope for Phase 02 as WebGPU support in mobile/hybrid WebViews remains inconsistent. CommonJS (`three.cjs`) is excluded in favor of standard ES modules. Addons (`examples/jsm/`) are deferred to when typed bindings or loaders require specific extensions.

## Attribution
Copyright © 2010-2026 three.js authors. Licensed under the MIT License. The full license text is preserved in `src/Three.cs/wwwroot/three/LICENSE` and shipped in the NuGet package and static web assets.

## Asset Update Procedure
1. In the `lib/three.js` submodule, checkout or update to the approved upstream revision.
2. Run the update script from repository root:
   ```powershell
   ./scripts/Sync-ThreeRuntime.ps1
   ```
3. To verify integrity without writing files (used by CI and tests):
   ```powershell
   ./scripts/Sync-ThreeRuntime.ps1 -Check
   ```
   The script verifies SHA-256 hashes, byte sizes, and provenance metadata (`upstream.json` and `three-upstream.js`).

## Known Limitations and Unsupported Features
- **WebGPU / TSL:** Not bundled in this phase.
- **Examples / JSM Addons:** Loaders (GLTFLoader, OBJLoader) and controls (OrbitControls) are not bundled in the core runtime; they will be integrated as modular extensions in future phases.
- **Node.js Canvas:** The runtime is intended for browser and WebView environments. In non-browser environments (e.g. headless Node unit tests), DOM/Canvas-dependent rendering requires appropriate polyfills.

