// Three.cs Three.js runtime â€” host-independent loading/initialization state machine.
//
// This module is an implementation detail of `three-runtime.js`, which is the only supported entry point.
// It is a factory so that the exact same logic can be exercised with a real Three.js module, with a failing
// importer, or with a controllable environment (tests). It performs no DOM or host-specific access by default
// other than an optional WebGL2 probe.

/** Version of the JavaScript contract exposed by `three-runtime.js`. Increment on breaking changes. */
export const CONTRACT_VERSION = 1;

/** Lifecycle states reported by `getState()`. */
export const RuntimeState = Object.freeze({
  Uninitialized: 'uninitialized',
  Initializing: 'initializing',
  Ready: 'ready',
  Failed: 'failed',
  Disposed: 'disposed',
});

/** Stable error codes reported in `error.code`. */
export const ErrorCode = Object.freeze({
  /** The Three.js module (or one of its dependencies) could not be fetched/evaluated. */
  ImportFailed: 'import-failed',
  /** The module loaded but does not provide the symbols required by this contract. */
  IncompatibleRuntime: 'incompatible-runtime',
  /** The module loaded but the post-load smoke verification failed. */
  InitializationFailed: 'initialization-failed',
  /** A query that requires a ready runtime was made before initialization completed. */
  NotInitialized: 'not-initialized',
  /** The runtime was disposed while initialization was in flight. */
  Disposed: 'disposed',
});

/** Failure stages reported in `error.stage`. */
export const ErrorStage = Object.freeze({
  Import: 'import',
  Verify: 'verify',
  Initialize: 'initialize',
  Query: 'query',
});

/** Error thrown by queries that cannot be answered (never thrown by `initialize`). */
export class ThreeRuntimeError extends Error {
  /**
   * @param {string} code One of {@link ErrorCode}.
   * @param {string} stage One of {@link ErrorStage}.
   * @param {string} message Human readable description.
   */
  constructor(code, stage, message) {
    super(message);
    this.name = 'ThreeRuntimeError';
    this.code = code;
    this.stage = stage;
  }
}

/**
 * Capability groups verified against the loaded module. A group is available only when every listed symbol
 * is exported. Groups marked `required` must be available or initialization fails.
 */
export const capabilityDefinitions = Object.freeze([
  { name: 'core', required: true, symbols: ['REVISION', 'EventDispatcher', 'MathUtils', 'Vector3', 'Matrix4', 'Quaternion', 'Euler', 'Color', 'Clock'] },
  { name: 'scene-graph', required: true, symbols: ['Object3D', 'Group', 'Scene', 'Mesh'] },
  { name: 'cameras', required: false, symbols: ['Camera', 'PerspectiveCamera', 'OrthographicCamera'] },
  { name: 'geometries', required: false, symbols: ['BufferGeometry', 'BoxGeometry', 'SphereGeometry', 'PlaneGeometry'] },
  { name: 'materials', required: false, symbols: ['Material', 'MeshBasicMaterial', 'MeshStandardMaterial', 'ShaderMaterial'] },
  { name: 'textures', required: false, symbols: ['Texture', 'CanvasTexture', 'DataTexture', 'TextureLoader'] },
  { name: 'lights', required: false, symbols: ['Light', 'AmbientLight', 'DirectionalLight', 'PointLight', 'SpotLight'] },
  { name: 'renderer-webgl', required: false, symbols: ['WebGLRenderer', 'WebGLRenderTarget'] },
]);

const defaultNow = () => globalThis.performance.now();

/**
 * Checks whether a WebGL2 context can be created in the current environment. The context is released
 * immediately. No renderer is created.
 * @returns {boolean}
 */
export function probeWebGL2() {
  try {
    let canvas = null;
    if (typeof globalThis.OffscreenCanvas === 'function') {
      canvas = new globalThis.OffscreenCanvas(1, 1);
    } else if (typeof globalThis.document !== 'undefined') {
      canvas = globalThis.document.createElement('canvas');
    }

    const context = canvas === null ? null : canvas.getContext('webgl2');
    if (!context) {
      return false;
    }

    context.getExtension('WEBGL_lose_context')?.loseContext();
    return true;
  } catch {
    return false;
  }
}

/**
 * Builds and uses a minimal scene graph to prove the loaded module is functional, then releases it.
 * @param {object} three The loaded module namespace.
 */
export function smokeTest(three) {
  const geometry = new three.BoxGeometry(1, 1, 1);
  const material = new three.MeshBasicMaterial();
  try {
    const scene = new three.Scene();
    const mesh = new three.Mesh(geometry, material);
    mesh.position.set(1, 2, 3);
    scene.add(mesh);
    const camera = new three.PerspectiveCamera(60, 1, 0.1, 10);
    camera.position.set(0, 0, 5);
    scene.updateMatrixWorld(true);
    camera.updateMatrixWorld(true);

    const world = mesh.getWorldPosition(new three.Vector3());
    if (world.x !== 1 || world.y !== 2 || world.z !== 3) {
      throw new Error(`Unexpected world position (${world.x}, ${world.y}, ${world.z}).`);
    }
  } finally {
    geometry.dispose();
    material.dispose();
  }
}

function describeError(error) {
  if (error instanceof Error && error.message) {
    return error.message;
  }

  return String(error);
}

function failure(code, stage, message) {
  return { code, stage, message };
}

/**
 * Creates a runtime instance.
 * @param {object} options
 * @param {{name:string, version:string, revision:string, commit:string, license:string, repository:string}} options.upstream Provenance metadata.
 * @param {() => Promise<object>} options.importThree Loads the Three.js module namespace.
 * @param {() => number} [options.now] Monotonic clock in milliseconds (`performance.now()` by default).
 * @param {() => boolean} [options.webgl2Probe] WebGL2 availability probe.
 * @param {(three: object) => void} [options.verify] Smoke verification; throws on failure.
 */
export function createRuntime({ upstream, importThree, now = defaultNow, webgl2Probe = probeWebGL2, verify = smokeTest }) {
  let status = RuntimeState.Uninitialized;
  let three = null;
  let capabilities = null;
  let timings = null;
  let error = null;
  let pending = null;
  let generation = 0;

  function snapshotResult() {
    return {
      ready: status === RuntimeState.Ready,
      status,
      upstream: getUpstreamInfo(),
      capabilities,
      timings,
      error,
    };
  }

  function evaluateCapabilities(module) {
    const available = [];
    const unavailable = [];
    let missingRequired = null;
    for (const definition of capabilityDefinitions) {
      const missing = definition.symbols.filter((symbol) => !(symbol in module));
      if (missing.length === 0) {
        available.push(definition.name);
      } else {
        unavailable.push(definition.name);
        if (definition.required && missingRequired === null) {
          missingRequired = `Required capability '${definition.name}' is missing exports: ${missing.join(', ')}.`;
        }
      }
    }

    return { available, unavailable, missingRequired };
  }

  async function run() {
    const startedAt = now();
    let importedAt;
    let module;
    try {
      module = await importThree();
      importedAt = now();
    } catch (cause) {
      importedAt = now();
      const elapsed = importedAt - startedAt;
      return { error: failure(ErrorCode.ImportFailed, ErrorStage.Import, describeError(cause)), timings: { importMs: elapsed, initializeMs: 0, totalMs: elapsed } };
    }

    const finish = (extra) => {
      const finishedAt = now();
      return { ...extra, timings: { importMs: importedAt - startedAt, initializeMs: finishedAt - importedAt, totalMs: finishedAt - startedAt } };
    };

    if (module === null || typeof module !== 'object') {
      return finish({ error: failure(ErrorCode.IncompatibleRuntime, ErrorStage.Verify, 'The Three.js module did not provide a module namespace.') });
    }

    const evaluation = evaluateCapabilities(module);
    if (evaluation.missingRequired !== null) {
      return finish({ error: failure(ErrorCode.IncompatibleRuntime, ErrorStage.Verify, evaluation.missingRequired) });
    }

    let webgl2;
    try {
      verify(module);
      webgl2 = Boolean(webgl2Probe());
    } catch (cause) {
      return finish({ error: failure(ErrorCode.InitializationFailed, ErrorStage.Initialize, describeError(cause)) });
    }

    return finish({
      module,
      capabilities: {
        available: evaluation.available,
        unavailable: evaluation.unavailable,
        environment: { webgl2 },
      },
    });
  }

  /**
   * Imports and verifies the Three.js module. Never rejects: failures are reported in the result.
   * Concurrent calls share one initialization; calling it again once ready is a no-op that returns the same state.
   */
  function initialize() {
    if (status === RuntimeState.Ready) {
      return Promise.resolve(snapshotResult());
    }

    if (pending !== null) {
      return pending;
    }

    status = RuntimeState.Initializing;
    error = null;
    const token = ++generation;
    const current = run().then((outcome) => {
      if (token !== generation) {
        // dispose() (or a newer initialize) superseded this run: discard its outcome without touching state.
        return {
          ready: false,
          status,
          upstream: getUpstreamInfo(),
          capabilities: null,
          timings: null,
          error: failure(ErrorCode.Disposed, ErrorStage.Initialize, 'The runtime was disposed while it was initializing.'),
        };
      }

      pending = null;
      timings = outcome.timings;
      if (outcome.error) {
        status = RuntimeState.Failed;
        error = outcome.error;
      } else {
        three = outcome.module;
        capabilities = outcome.capabilities;
        status = RuntimeState.Ready;
      }

      return snapshotResult();
    });

    pending = current;
    return current;
  }

  /** Returns the lifecycle state, the last timings and the last failure (if any). */
  function getState() {
    return { contractVersion: CONTRACT_VERSION, status, ready: status === RuntimeState.Ready, timings, error };
  }

  /** Returns provenance of the shipped Three.js build. `loadedRevision` is reported by the loaded module itself. */
  function getUpstreamInfo() {
    return { ...upstream, loadedRevision: three === null ? null : String(three.REVISION) };
  }

  /** Returns the capabilities that were effectively verified. Requires a ready runtime. */
  function getCapabilities() {
    if (status !== RuntimeState.Ready) {
      throw new ThreeRuntimeError(ErrorCode.NotInitialized, ErrorStage.Query, `The runtime is '${status}'; initialize it before querying capabilities.`);
    }

    return capabilities;
  }

  /**
   * Releases references held by the runtime. Idempotent. An in-flight initialization is abandoned.
   * The browser keeps the evaluated module in its module map; `initialize()` may be called again afterwards.
   */
  function dispose() {
    if (status === RuntimeState.Disposed) {
      return;
    }

    generation++;
    pending = null;
    three = null;
    capabilities = null;
    timings = null;
    error = null;
    status = RuntimeState.Disposed;
  }

  return { contractVersion: CONTRACT_VERSION, initialize, getState, getUpstreamInfo, getCapabilities, dispose };
}
