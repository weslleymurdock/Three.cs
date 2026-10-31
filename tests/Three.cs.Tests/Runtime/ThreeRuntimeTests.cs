using Microsoft.Extensions.Options;
using Three.Hosting;
using Three.Interop;
using Three.Lifecycle;
using Three.Runtime;
using Three.Tests.TestDoubles;

namespace Three.Tests.Runtime;

public class ThreeRuntimeTests
{
    [Fact]
    public void Constructor_NullBridge_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ThreeRuntime(null!));
    }

    [Fact]
    public void InitialState_IsUninitialized()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);
        var runtime = new ThreeRuntime(bridge);

        Assert.Equal(ThreeLifecycleState.Uninitialized, runtime.State);
        Assert.Same(bridge, runtime.Bridge);
        Assert.NotNull(runtime.Options);
    }

    [Fact]
    public async Task InitializeAsync_EnsuresBridgeInitialized_AndTransitionsToInitialized()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };
        var bridge = new JsBridge(jsRuntime);
        var runtime = new ThreeRuntime(bridge);

        await runtime.InitializeAsync();

        Assert.Equal(ThreeLifecycleState.Initialized, runtime.State);
        Assert.True(bridge.IsReady);
    }

    [Fact]
    public async Task DisposeAsync_DisposesBridge_AndTransitionsToDisposed()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };
        var bridge = new JsBridge(jsRuntime);
        var runtime = new ThreeRuntime(bridge);

        await runtime.InitializeAsync();
        Assert.True(bridge.IsReady);

        await runtime.DisposeAsync();

        Assert.Equal(ThreeLifecycleState.Disposed, runtime.State);
        Assert.False(bridge.IsReady);
    }

    [Fact]
    public void CustomOptions_AreAccessibleOnRuntime()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);
        var options = new ThreeOptions
        {
            CanvasIdPrefix = "app-canvas-"
        };

        var runtime = new ThreeRuntime(bridge, Options.Create(options));

        Assert.Equal("app-canvas-", runtime.Options.CanvasIdPrefix);
    }
}

