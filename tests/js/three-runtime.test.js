import { test, describe } from 'node:test';
import assert from 'node:assert/strict';
import {
  CONTRACT_VERSION,
  RuntimeState,
  ErrorCode,
  ErrorStage,
  ThreeRuntimeError,
  createRuntime,
} from '../../src/Three.cs/wwwroot/js/three-runtime-core.js';
import { upstream } from '../../src/Three.cs/wwwroot/js/three-upstream.js';

describe('JavaScript Three.js Runtime Facade', () => {
  test('initializes successfully with real Three.js module', async () => {
    const runtime = createRuntime({
      upstream,
      importThree: () => import('../../src/Three.cs/wwwroot/three/three.module.js'),
    });

    assert.equal(runtime.contractVersion, CONTRACT_VERSION);
    const initial = runtime.getState();
    assert.equal(initial.status, RuntimeState.Uninitialized);
    assert.equal(initial.ready, false);

    const result = await runtime.initialize();
    assert.equal(result.ready, true);
    assert.equal(result.status, RuntimeState.Ready);
    assert.equal(result.error, null);
    assert.ok(result.timings.totalMs >= 0);

    const state = runtime.getState();
    assert.equal(state.ready, true);
    assert.equal(state.status, RuntimeState.Ready);

    const info = runtime.getUpstreamInfo();
    assert.equal(info.name, 'three');
    assert.equal(info.version, '0.187.0');
    assert.equal(info.revision, '187');
    assert.equal(info.loadedRevision, '187');

    const capabilities = runtime.getCapabilities();
    assert.ok(capabilities.available.includes('core'));
    assert.ok(capabilities.available.includes('scene-graph'));
    assert.ok(capabilities.available.includes('renderer-webgl'));
    assert.ok(capabilities.available.includes('geometries'));
    assert.ok(capabilities.available.includes('materials'));

    // Second initialize is idempotent and returns immediately
    const second = await runtime.initialize();
    assert.equal(second.ready, true);
    assert.equal(second.status, RuntimeState.Ready);

    runtime.dispose();
    const disposedState = runtime.getState();
    assert.equal(disposedState.status, RuntimeState.Disposed);
    assert.equal(disposedState.ready, false);
    assert.throws(() => runtime.getCapabilities(), (err) => err instanceof ThreeRuntimeError && err.code === ErrorCode.NotInitialized);
  });

  test('handles concurrent initialize() calls safely', async () => {
    let importCount = 0;
    const runtime = createRuntime({
      upstream,
      importThree: async () => {
        importCount++;
        await new Promise((r) => setTimeout(r, 10));
        return import('../../src/Three.cs/wwwroot/three/three.module.js');
      },
    });

    const [r1, r2, r3] = await Promise.all([
      runtime.initialize(),
      runtime.initialize(),
      runtime.initialize(),
    ]);

    assert.equal(importCount, 1);
    assert.equal(r1.ready, true);
    assert.equal(r2.ready, true);
    assert.equal(r3.ready, true);
    assert.equal(r1.status, RuntimeState.Ready);
  });

  test('reports import failure when module cannot be loaded', async () => {
    const runtime = createRuntime({
      upstream,
      importThree: () => Promise.reject(new Error('Failed to fetch module')),
    });

    const result = await runtime.initialize();
    assert.equal(result.ready, false);
    assert.equal(result.status, RuntimeState.Failed);
    assert.equal(result.error.code, ErrorCode.ImportFailed);
    assert.equal(result.error.stage, ErrorStage.Import);
    assert.ok(result.error.message.includes('Failed to fetch module'));

    const state = runtime.getState();
    assert.equal(state.status, RuntimeState.Failed);
    assert.equal(state.ready, false);
    assert.throws(() => runtime.getCapabilities(), (err) => err instanceof ThreeRuntimeError && err.code === ErrorCode.NotInitialized);
  });

  test('reports incompatible runtime when required exports are missing', async () => {
    const runtime = createRuntime({
      upstream,
      importThree: async () => ({
        // Missing core symbols
        REVISION: '187',
      }),
    });

    const result = await runtime.initialize();
    assert.equal(result.ready, false);
    assert.equal(result.status, RuntimeState.Failed);
    assert.equal(result.error.code, ErrorCode.IncompatibleRuntime);
    assert.equal(result.error.stage, ErrorStage.Verify);
    assert.ok(result.error.message.includes('Required capability'));
  });

  test('reports initialization failure when verify throws', async () => {
    const runtime = createRuntime({
      upstream,
      importThree: () => import('../../src/Three.cs/wwwroot/three/three.module.js'),
      verify: () => {
        throw new Error('Smoke test failed');
      },
    });

    const result = await runtime.initialize();
    assert.equal(result.ready, false);
    assert.equal(result.status, RuntimeState.Failed);
    assert.equal(result.error.code, ErrorCode.InitializationFailed);
    assert.equal(result.error.stage, ErrorStage.Initialize);
    assert.ok(result.error.message.includes('Smoke test failed'));
  });

  test('reports disposed when disposed while initialize is in flight', async () => {
    const runtime = createRuntime({
      upstream,
      importThree: async () => {
        await new Promise((r) => setTimeout(r, 20));
        return import('../../src/Three.cs/wwwroot/three/three.module.js');
      },
    });

    const initPromise = runtime.initialize();
    runtime.dispose();

    const result = await initPromise;
    assert.equal(result.ready, false);
    assert.equal(result.error.code, ErrorCode.Disposed);
    assert.equal(runtime.getState().status, RuntimeState.Disposed);
  });
});

