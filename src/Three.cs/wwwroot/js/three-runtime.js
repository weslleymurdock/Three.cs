// Three.cs Three.js runtime — stable entry point.
//
// Consume this module from any host through its Static Web Asset path:
//
//   const runtime = await import('./_content/Three.cs/js/three-runtime.js');
//   const result = await runtime.initialize();
//
// Contract (version `contractVersion`):
//   initialize()       -> Promise<InitializationResult>  Loads and verifies Three.js. Never rejects; failures are
//                                                        reported in `result.error`. Concurrent calls share one run.
//   getState()         -> { contractVersion, status, ready, timings, error }
//   getUpstreamInfo()  -> { name, version, revision, commit, license, repository, loadedRevision }
//   getCapabilities()  -> { available: string[], unavailable: string[], environment: { webgl2: boolean } }
//                         Throws ThreeRuntimeError(code 'not-initialized') unless the runtime is ready.
//   dispose()          -> void                           Releases runtime references. Idempotent.
//
// InitializationResult: { ready, status, upstream, capabilities|null, timings|null, error|null }
//   timings: { importMs, initializeMs, totalMs }, measured in this realm with performance.now().
//   error:   { code, stage, message } where code is one of import-failed, incompatible-runtime,
//            initialization-failed, disposed.
//
// Importing this module is cheap: the Three.js build is loaded lazily by initialize().

import { upstream } from './three-upstream.js';
import { CONTRACT_VERSION, createRuntime } from './three-runtime-core.js';

const runtime = createRuntime({
  upstream,
  importThree: () => import('../three/three.module.js'),
});

export const contractVersion = CONTRACT_VERSION;
export const initialize = runtime.initialize;
export const getState = runtime.getState;
export const getUpstreamInfo = runtime.getUpstreamInfo;
export const getCapabilities = runtime.getCapabilities;
export const dispose = runtime.dispose;

