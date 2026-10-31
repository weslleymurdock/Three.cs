using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Three.Hosting;
using Three.Interop;
using Three.Tests.TestDoubles;

namespace Three.Tests.Interop;

public class JsBridgeTests
{
    [Fact]
    public void Constructor_NullJsRuntime_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new JsBridge(null!));
    }

    [Fact]
    public void IsReady_InitiallyFalse()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task InvokeVoidAsync_BeforeInitialization_ThrowsJsBridgeException()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.InvokeVoidAsync("test"));
        Assert.Contains("not ready", ex.Message);
    }

    [Fact]
    public async Task InvokeAsync_BeforeInitialization_ThrowsJsBridgeException()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.InvokeAsync<string>("test"));
        Assert.Contains("not ready", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvokeVoidAsync_InvalidIdentifier_ThrowsArgumentException(string? invalidIdentifier)
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await bridge.InvokeVoidAsync(invalidIdentifier!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvokeAsync_InvalidIdentifier_ThrowsArgumentException(string? invalidIdentifier)
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await bridge.InvokeAsync<int>(invalidIdentifier!));
    }

    [Fact]
    public async Task EnsureInitializedAsync_ImportsScriptAndCallsInitialize()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var options = Options.Create(new ThreeOptions { ScriptPath = "./custom-bridge.js" });
        var bridge = new JsBridge(jsRuntime, options);

        await bridge.EnsureInitializedAsync();

        Assert.True(bridge.IsReady);
        Assert.Contains("import", jsRuntime.Invocations);
        Assert.Contains("initialize", module.Invocations);
    }

    [Fact]
    public async Task EnsureInitializedAsync_IsIdempotent()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);

        await bridge.EnsureInitializedAsync();
        await bridge.EnsureInitializedAsync();

        Assert.Single(jsRuntime.Invocations, x => x == "import");
        Assert.Single(module.Invocations, x => x == "initialize");
    }

    [Fact]
    public async Task EnsureInitializedAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await bridge.EnsureInitializedAsync(cts.Token));
        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task EnsureInitializedAsync_WhenJsExceptionThrown_WrapsInJsBridgeException()
    {
        var jsRuntime = new TestJsRuntime
        {
            ErrorHandler = (identifier, args) => new JSException("Script load failed 404")
        };

        var bridge = new JsBridge(jsRuntime);

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.EnsureInitializedAsync());
        Assert.Contains("Failed to initialize Three.cs JavaScript bridge", ex.Message);
        Assert.IsType<JSException>(ex.InnerException);
    }

    [Fact]
    public async Task EnsureInitializedAsync_WhenGenericExceptionThrown_WrapsInJsBridgeException()
    {
        var jsRuntime = new TestJsRuntime
        {
            ErrorHandler = (identifier, args) => new InvalidOperationException("Host network failure")
        };

        var bridge = new JsBridge(jsRuntime);

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.EnsureInitializedAsync());
        Assert.Contains("unexpected error occurred", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task InvokeVoidAsync_ExecutesFunctionOnModule()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        await bridge.InvokeVoidAsync("render", CancellationToken.None, "canvas-1");

        Assert.Contains("render", module.Invocations);
    }

    [Fact]
    public async Task InvokeVoidAsync_WhenJsExceptionThrown_WrapsInJsBridgeException()
    {
        var module = new TestJsObjectReference
        {
            ErrorHandler = (identifier, args) => identifier == "render" ? new JSException("WebGL context lost") : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.InvokeVoidAsync("render"));
        Assert.Contains("Failed to invoke JavaScript function 'render'", ex.Message);
        Assert.IsType<JSException>(ex.InnerException);
    }

    [Fact]
    public async Task InvokeVoidAsync_WhenGenericExceptionThrown_WrapsInJsBridgeException()
    {
        var module = new TestJsObjectReference
        {
            ErrorHandler = (identifier, args) => identifier == "render" ? new InvalidOperationException("Process aborted") : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.InvokeVoidAsync("render"));
        Assert.Contains("unexpected error occurred while invoking 'render'", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsDeserializedResult()
    {
        var module = new TestJsObjectReference
        {
            InvokeHandler = (identifier, args) => identifier == "getFps" ? 60 : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var result = await bridge.InvokeAsync<int>("getFps");
        Assert.Equal(60, result);
    }

    [Fact]
    public async Task InvokeAsync_WhenJsExceptionThrown_WrapsInJsBridgeException()
    {
        var module = new TestJsObjectReference
        {
            ErrorHandler = (identifier, args) => identifier == "getFps" ? new JSException("Null reference") : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.InvokeAsync<int>("getFps"));
        Assert.Contains("Failed to invoke JavaScript function 'getFps'", ex.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_WrapsInJsBridgeException()
    {
        var module = new TestJsObjectReference
        {
            ErrorHandler = (identifier, args) => identifier == "getFps" ? new FormatException("Corrupted JSON") : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var ex = await Assert.ThrowsAsync<JsBridgeException>(async () => await bridge.InvokeAsync<int>("getFps"));
        Assert.Contains("unexpected error occurred", ex.Message);
    }

    [Fact]
    public async Task DisposeAsync_DisposesModuleAndMarksNotReady()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();
        Assert.True(bridge.IsReady);

        await bridge.DisposeAsync();

        Assert.False(bridge.IsReady);
        Assert.Contains("dispose", module.Invocations);
        Assert.True(module.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        await bridge.DisposeAsync();
        await bridge.DisposeAsync();

        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task DisposeAsync_HandlesModuleDisposalErrorsGracefully()
    {
        var module = new TestJsObjectReference
        {
            ThrowOnDispose = true
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        // Should not throw even when module disposal errors
        await bridge.DisposeAsync();
        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task DisposeAsync_WhenNeverInitialized_DoesNotThrow()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);

        await bridge.DisposeAsync();
        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task DisposeAsync_WhenModuleDisposeMethodThrows_SuppressesError()
    {
        var module = new TestJsObjectReference
        {
            ErrorHandler = (identifier, args) => identifier == JsInteropConstants.DisposeMethod ? new JSException("Dispose failed in JS") : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        await bridge.DisposeAsync();
        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task EnsureInitializedAsync_ConcurrentCalls_CoordinateSafely()
    {
        var tcs = new TaskCompletionSource();
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            AsyncInvokeHandler = async (identifier, args) =>
            {
                if (identifier == "import")
                {
                    await tcs.Task;
                    return module;
                }
                return null;
            }
        };

        var bridge = new JsBridge(jsRuntime);

        var task1 = bridge.EnsureInitializedAsync();
        var task2 = bridge.EnsureInitializedAsync();

        tcs.SetResult();
        await Task.WhenAll(task1.AsTask(), task2.AsTask());

        Assert.True(bridge.IsReady);
        Assert.Single(jsRuntime.Invocations, x => x == "import");
    }

    [Fact]
    public async Task DisposeAsync_ConcurrentCalls_CoordinateSafely()
    {
        var tcs = new TaskCompletionSource();
        var module = new TestJsObjectReference
        {
            AsyncDisposeHandler = async () =>
            {
                await tcs.Task;
            }
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var task1 = bridge.DisposeAsync();
        var task2 = bridge.DisposeAsync();

        tcs.SetResult();
        await Task.WhenAll(task1.AsTask(), task2.AsTask());

        Assert.False(bridge.IsReady);
    }

    [Fact]
    public async Task InvokeVoidAsync_WithExplicitArgs_PassesArguments()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        await bridge.InvokeVoidAsync("resize", CancellationToken.None, 800, 600);

        Assert.Contains("resize", module.Invocations);
    }

    [Fact]
    public async Task InvokeAsync_WithExplicitArgs_PassesArguments()
    {
        var module = new TestJsObjectReference
        {
            InvokeHandler = (identifier, args) => identifier == "calculate" ? 42 : null
        };
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        var result = await bridge.InvokeAsync<int>("calculate", CancellationToken.None, "param1", 100);

        Assert.Equal(42, result);
        Assert.Contains("calculate", module.Invocations);
    }

    [Fact]
    public async Task InvokeVoidAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await bridge.InvokeVoidAsync("render", cts.Token));
    }

    [Fact]
    public async Task InvokeAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var module = new TestJsObjectReference();
        var jsRuntime = new TestJsRuntime
        {
            InvokeHandler = (identifier, args) => module
        };

        var bridge = new JsBridge(jsRuntime);
        await bridge.EnsureInitializedAsync();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await bridge.InvokeAsync<int>("getFps", cts.Token));
    }

    [Fact]
    public async Task Methods_ThrowObjectDisposedException_AfterDisposal()
    {
        var jsRuntime = new TestJsRuntime();
        var bridge = new JsBridge(jsRuntime);
        await bridge.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await bridge.EnsureInitializedAsync());
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await bridge.InvokeVoidAsync("test"));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await bridge.InvokeAsync<string>("test"));
    }
}
