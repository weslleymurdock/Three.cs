using System.Text.Json;
using Microsoft.JSInterop;
using Three.Interop;
using Three.Runtime;

namespace Three.Tests;

public class ThreeRuntimeLifecycleTests
{
    private static readonly ThreeUpstreamInfo SampleUpstream = new(
        Name: "three",
        Version: "0.187.0",
        Revision: "187",
        Commit: "cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168",
        License: "MIT",
        Repository: "https://github.com/mrdoob/three.js",
        LoadedRevision: "187");

    private static readonly ThreeRuntimeCapabilities SampleCapabilities = new(
        Available: ["core", "scene-graph", "renderer-webgl"],
        Unavailable: [],
        Environment: new ThreeRuntimeEnvironment(WebGL2: true));

    private static readonly ThreeRuntimeTimings SampleTimings = new(
        ImportMs: 25.0,
        InitializeMs: 5.0,
        TotalMs: 30.0);

    [Fact]
    public void Constructor_ThrowsOnNullJsRuntime()
    {
        Assert.Throws<ArgumentNullException>(() => new ThreeRuntime(null!));
    }

    [Fact]
    public void InitialState_IsUninitialized()
    {
        var js = new ScriptedJsRuntime((id, args) => null);
        var runtime = new ThreeRuntime(js);

        Assert.Equal(ThreeRuntimeState.Uninitialized, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_SuccessPath_TransitionsToReadyAndReturnsData()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Initialize)
            {
                return new ThreeRuntimeInitializeResponse(
                    Ready: true,
                    Status: "ready",
                    Upstream: SampleUpstream,
                    Capabilities: SampleCapabilities,
                    Timings: SampleTimings,
                    Error: null);
            }
            if (id == JsInteropConstants.GetUpstreamInfo) return SampleUpstream;
            if (id == JsInteropConstants.GetCapabilities) return SampleCapabilities;
            return null;
        });

        var js = new ScriptedJsRuntime((id, args) =>
        {
            if (id == JsInteropConstants.ImportFunction) return module;
            throw new InvalidOperationException($"Unexpected JS call: {id}");
        });

        await using var runtime = new ThreeRuntime(js);

        var result = await runtime.InitializeAsync();

        Assert.Equal(ThreeRuntimeState.Ready, runtime.State);
        Assert.Equal(SampleUpstream, result.Upstream);
        Assert.Equal(SampleCapabilities, result.Capabilities);
        Assert.Equal(SampleTimings, result.Timings);

        // Check query methods when Ready
        var upstream = await runtime.GetUpstreamInfoAsync();
        Assert.Equal(SampleUpstream, upstream);

        var capabilities = await runtime.GetCapabilitiesAsync();
        Assert.Equal(SampleCapabilities, capabilities);

        // Second initialize is idempotent and returns cached result without re-invoking initialize
        var second = await runtime.InitializeAsync();
        Assert.Same(result, second);
    }

    [Fact]
    public async Task InitializeAsync_ConcurrentCalls_AreSerialized()
    {
        int initCount = 0;
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Initialize)
            {
                Interlocked.Increment(ref initCount);
                Thread.Sleep(50);
                return new ThreeRuntimeInitializeResponse(
                    Ready: true,
                    Status: "ready",
                    Upstream: SampleUpstream,
                    Capabilities: SampleCapabilities,
                    Timings: SampleTimings,
                    Error: null);
            }
            return null;
        });

        var js = new ScriptedJsRuntime((id, args) => module);
        await using var runtime = new ThreeRuntime(js);

        var task1 = Task.Run(() => runtime.InitializeAsync());
        var task2 = Task.Run(() => runtime.InitializeAsync());
        var results = await Task.WhenAll(task1, task2);

        Assert.Equal(1, initCount);
        Assert.Same(results[0], results[1]);
        Assert.Equal(ThreeRuntimeState.Ready, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsOnModuleImportFailure()
    {
        var js = new ScriptedJsRuntime((id, args) =>
        {
            throw new JSException("Failed to fetch dynamically imported module");
        });

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.ModuleImportFailed, ex.Code);
        Assert.Equal("import", ex.Stage);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsOnJsExceptionDuringInitialize()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Dispose) return null;
            throw new JSException("ReferenceError: window is not defined");
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.JavaScriptError, ex.Code);
        Assert.Equal("initialize", ex.Stage);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsOnJsonException()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Dispose) return null;
            throw new JsonException("Invalid JSON format in payload");
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.InvalidResponse, ex.Code);
        Assert.Equal("initialize", ex.Stage);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsOnNullResponse()
    {
        var module = new ScriptedJsObjectReference((id, args) => null);
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.InvalidResponse, ex.Code);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsWhenReadyIsFalse_WithError()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            return new ThreeRuntimeInitializeResponse(
                Ready: false,
                Status: "failed",
                Upstream: SampleUpstream,
                Capabilities: null,
                Timings: null,
                Error: new ThreeRuntimeErrorInfo("incompatible-runtime", "verify", "Required symbol missing"));
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal("incompatible-runtime", ex.Code);
        Assert.Equal("verify", ex.Stage);
        Assert.Equal("Required symbol missing", ex.Message);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsWhenReadyIsFalse_WithoutError()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            return new ThreeRuntimeInitializeResponse(
                Ready: false,
                Status: "failed",
                Upstream: SampleUpstream,
                Capabilities: null,
                Timings: null,
                Error: null);
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.InvalidResponse, ex.Code);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_ThrowsWhenReadyIsTrue_ButCapabilitiesOrTimingsNull()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            return new ThreeRuntimeInitializeResponse(
                Ready: true,
                Status: "ready",
                Upstream: SampleUpstream,
                Capabilities: null,
                Timings: SampleTimings,
                Error: null);
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        var ex = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.InitializeAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.InvalidResponse, ex.Code);
        Assert.Equal(ThreeRuntimeState.Failed, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_CancellationBeforeLock_RestoresState()
    {
        var js = new ScriptedJsRuntime((id, args) => null);
        await using var runtime = new ThreeRuntime(js);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runtime.InitializeAsync(cts.Token));
        Assert.Equal(ThreeRuntimeState.Uninitialized, runtime.State);
    }

    [Fact]
    public async Task InitializeAsync_CancellationDuringOperation_RestoresState()
    {
        using var cts = new CancellationTokenSource();
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Initialize)
            {
                cts.Cancel();
                cts.Token.ThrowIfCancellationRequested();
            }
            if (id == JsInteropConstants.Dispose) return null;
            return null;
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runtime.InitializeAsync(cts.Token));
        Assert.Equal(ThreeRuntimeState.Uninitialized, runtime.State);
    }

    [Fact]
    public async Task Queries_ThrowWhenNotReady()
    {
        var js = new ScriptedJsRuntime((id, args) => null);
        await using var runtime = new ThreeRuntime(js);

        var exUpstream = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.GetUpstreamInfoAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.NotInitialized, exUpstream.Code);

        var exCaps = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.GetCapabilitiesAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.NotInitialized, exCaps.Code);
    }

    [Fact]
    public async Task Queries_ThrowWhenJsThrows()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Initialize)
            {
                return new ThreeRuntimeInitializeResponse(
                    Ready: true,
                    Status: "ready",
                    Upstream: SampleUpstream,
                    Capabilities: SampleCapabilities,
                    Timings: SampleTimings,
                    Error: null);
            }
            if (id == JsInteropConstants.Dispose) return null;
            throw new JSException("Query error");
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        await using var runtime = new ThreeRuntime(js);
        await runtime.InitializeAsync();

        var ex1 = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.GetUpstreamInfoAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.JavaScriptError, ex1.Code);

        var ex2 = await Assert.ThrowsAsync<ThreeRuntimeException>(() => runtime.GetCapabilitiesAsync());
        Assert.Equal(ThreeRuntimeFailureCodes.JavaScriptError, ex2.Code);
    }

    [Fact]
    public async Task DisposeAsync_ReleasesResources_AndIsIdempotent()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Initialize)
            {
                return new ThreeRuntimeInitializeResponse(
                    Ready: true,
                    Status: "ready",
                    Upstream: SampleUpstream,
                    Capabilities: SampleCapabilities,
                    Timings: SampleTimings,
                    Error: null);
            }
            return null;
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        var runtime = new ThreeRuntime(js);
        await runtime.InitializeAsync();

        Assert.False(module.Disposed);

        await runtime.DisposeAsync();

        Assert.Equal(ThreeRuntimeState.Disposed, runtime.State);
        Assert.True(module.Disposed);
        Assert.Contains(module.Invocations, inv => inv.Identifier == JsInteropConstants.Dispose);

        // Second dispose is idempotent
        await runtime.DisposeAsync();
        Assert.Equal(ThreeRuntimeState.Disposed, runtime.State);

        // Post-disposal operations throw ObjectDisposedException
        await Assert.ThrowsAsync<ObjectDisposedException>(() => runtime.InitializeAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => runtime.GetUpstreamInfoAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => runtime.GetCapabilitiesAsync());
    }

    [Fact]
    public async Task DisposeAsync_HandlesDisconnectedJsGracefully()
    {
        var module = new ScriptedJsObjectReference((id, args) =>
        {
            if (id == JsInteropConstants.Initialize)
            {
                return new ThreeRuntimeInitializeResponse(
                    Ready: true,
                    Status: "ready",
                    Upstream: SampleUpstream,
                    Capabilities: SampleCapabilities,
                    Timings: SampleTimings,
                    Error: null);
            }
            if (id == JsInteropConstants.Dispose)
            {
                throw new JSDisconnectedException("Circuit disconnected");
            }
            return null;
        });
        var js = new ScriptedJsRuntime((id, args) => module);

        var runtime = new ThreeRuntime(js);
        await runtime.InitializeAsync();

        // Should not throw even if JSDisconnectedException occurs
        await runtime.DisposeAsync();
        Assert.Equal(ThreeRuntimeState.Disposed, runtime.State);
        Assert.True(module.Disposed);
    }
}
