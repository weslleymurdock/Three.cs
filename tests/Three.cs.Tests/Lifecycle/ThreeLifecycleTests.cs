using Three.Lifecycle;

namespace Three.Tests.Lifecycle;

public class ThreeLifecycleTests
{
    [Fact]
    public void InitialState_IsUninitialized()
    {
        var lifecycle = new ThreeLifecycle();

        Assert.Equal(ThreeLifecycleState.Uninitialized, lifecycle.State);
        Assert.False(lifecycle.IsInitialized);
        Assert.False(lifecycle.IsDisposed);
    }

    [Fact]
    public void ThrowIfNotInitialized_ThrowsWhenUninitialized()
    {
        var lifecycle = new ThreeLifecycle("TestComponent");

        var ex = Assert.Throws<InvalidOperationException>(() => lifecycle.ThrowIfNotInitialized());
        Assert.Contains("TestComponent", ex.Message);
        Assert.Contains("not initialized", ex.Message);
    }

    [Fact]
    public async Task InitializeAsync_TransitionsToInitialized_AndExecutesAction()
    {
        var initialized = false;
        var lifecycle = new ThreeLifecycle(
            onInitialize: ct =>
            {
                initialized = true;
                return ValueTask.CompletedTask;
            });

        await lifecycle.InitializeAsync();

        Assert.True(initialized);
        Assert.Equal(ThreeLifecycleState.Initialized, lifecycle.State);
        Assert.True(lifecycle.IsInitialized);
        Assert.False(lifecycle.IsDisposed);

        // Verify ThrowIfNotInitialized does not throw
        lifecycle.ThrowIfNotInitialized();
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var invocationCount = 0;
        var lifecycle = new ThreeLifecycle(
            onInitialize: ct =>
            {
                invocationCount++;
                return ValueTask.CompletedTask;
            });

        await lifecycle.InitializeAsync();
        await lifecycle.InitializeAsync();

        Assert.Equal(1, invocationCount);
        Assert.True(lifecycle.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_WhenActionThrows_RevertsToUninitializedAndRethrows()
    {
        var lifecycle = new ThreeLifecycle(
            onInitialize: ct => throw new InvalidOperationException("Initialization failed"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await lifecycle.InitializeAsync());
        Assert.Equal("Initialization failed", ex.Message);
        Assert.Equal(ThreeLifecycleState.Uninitialized, lifecycle.State);
        Assert.False(lifecycle.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var lifecycle = new ThreeLifecycle();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await lifecycle.InitializeAsync(cts.Token));
        Assert.Equal(ThreeLifecycleState.Uninitialized, lifecycle.State);
    }

    [Fact]
    public async Task InitializeAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        var lifecycle = new ThreeLifecycle("TestComponent");
        await lifecycle.DisposeAsync();

        var ex = await Assert.ThrowsAsync<ObjectDisposedException>(async () => await lifecycle.InitializeAsync());
        Assert.Contains("TestComponent", ex.Message);
    }

    [Fact]
    public async Task ThrowIfNotInitialized_ThrowsObjectDisposedException_WhenDisposed()
    {
        var lifecycle = new ThreeLifecycle("TestComponent");
        await lifecycle.DisposeAsync();

        var ex = Assert.Throws<ObjectDisposedException>(() => lifecycle.ThrowIfNotInitialized());
        Assert.Contains("TestComponent", ex.Message);
    }

    [Fact]
    public async Task DisposeAsync_TransitionsToDisposed_AndExecutesAction()
    {
        var disposed = false;
        var lifecycle = new ThreeLifecycle(
            onDispose: () =>
            {
                disposed = true;
                return ValueTask.CompletedTask;
            });

        await lifecycle.DisposeAsync();

        Assert.True(disposed);
        Assert.Equal(ThreeLifecycleState.Disposed, lifecycle.State);
        Assert.False(lifecycle.IsInitialized);
        Assert.True(lifecycle.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        var disposeCount = 0;
        var lifecycle = new ThreeLifecycle(
            onDispose: () =>
            {
                disposeCount++;
                return ValueTask.CompletedTask;
            });

        await lifecycle.DisposeAsync();
        await lifecycle.DisposeAsync();

        Assert.Equal(1, disposeCount);
        Assert.True(lifecycle.IsDisposed);
    }

    [Fact]
    public async Task ThrowIfNotInitialized_ThrowsDuringInitialization()
    {
        var tcs = new TaskCompletionSource();
        ThreeLifecycle? lifecycle = null;

        lifecycle = new ThreeLifecycle(
            componentName: "SlowComponent",
            onInitialize: async ct =>
            {
                var ex = Assert.Throws<InvalidOperationException>(() => lifecycle!.ThrowIfNotInitialized());
                Assert.Contains("SlowComponent", ex.Message);
                Assert.Contains("Initializing", ex.Message);
                tcs.SetResult();
                await Task.CompletedTask;
            });

        await lifecycle.InitializeAsync();
        await tcs.Task;
    }

    [Fact]
    public async Task InitializeAsync_ConcurrentInvocations_AreSafelyCoordinated()
    {
        var tcs = new TaskCompletionSource();
        var count = 0;
        var lifecycle = new ThreeLifecycle(
            onInitialize: async ct =>
            {
                Interlocked.Increment(ref count);
                await tcs.Task;
            });

        var t1 = lifecycle.InitializeAsync();
        var t2 = lifecycle.InitializeAsync();

        tcs.SetResult();
        await Task.WhenAll(t1.AsTask(), t2.AsTask());

        Assert.Equal(1, count);
        Assert.True(lifecycle.IsInitialized);
    }

    [Fact]
    public async Task DisposeAsync_ConcurrentInvocations_AreSafelyCoordinated()
    {
        var tcs = new TaskCompletionSource();
        var count = 0;
        var lifecycle = new ThreeLifecycle(
            onDispose: async () =>
            {
                Interlocked.Increment(ref count);
                await tcs.Task;
            });

        var t1 = lifecycle.DisposeAsync();
        var t2 = lifecycle.DisposeAsync();

        tcs.SetResult();
        await Task.WhenAll(t1.AsTask(), t2.AsTask());

        Assert.Equal(1, count);
        Assert.True(lifecycle.IsDisposed);
    }

    [Fact]
    public async Task InitializeAsync_WhenDisposedWhileWaitingForLock_ThrowsObjectDisposedException()
    {
        var tcs = new TaskCompletionSource();
        var lifecycle = new ThreeLifecycle(
            componentName: "RaceComponent",
            onDispose: async () =>
            {
                await tcs.Task;
            });

        var disposeTask = lifecycle.DisposeAsync();
        var initTask = lifecycle.InitializeAsync();

        tcs.SetResult();
        await disposeTask;

        var ex = await Assert.ThrowsAsync<ObjectDisposedException>(async () => await initTask);
        Assert.Contains("RaceComponent", ex.Message);
    }

    [Fact]
    public async Task ThrowIfNotInitialized_ThrowsDuringDisposal()
    {
        ThreeLifecycle? lifecycle = null;
        lifecycle = new ThreeLifecycle(
            componentName: "DisposingComponent",
            onDispose: () =>
            {
                var ex = Assert.Throws<ObjectDisposedException>(() => lifecycle!.ThrowIfNotInitialized());
                Assert.Contains("DisposingComponent", ex.Message);
                return ValueTask.CompletedTask;
            });

        await lifecycle.DisposeAsync();
    }

    [Fact]
    public async Task InitializeAsync_WhenDisposing_ThrowsObjectDisposedException()
    {
        ThreeLifecycle? lifecycle = null;
        lifecycle = new ThreeLifecycle(
            componentName: "DisposingComponent",
            onDispose: async () =>
            {
                var ex = await Assert.ThrowsAsync<ObjectDisposedException>(async () => await lifecycle!.InitializeAsync());
                Assert.Contains("DisposingComponent", ex.Message);
            });

        await lifecycle.DisposeAsync();
    }

    [Fact]
    public async Task InitializeAndDispose_WorkWithoutCallbacks()
    {
        var lifecycle = new ThreeLifecycle();

        await lifecycle.InitializeAsync();
        Assert.True(lifecycle.IsInitialized);

        await lifecycle.DisposeAsync();
        Assert.True(lifecycle.IsDisposed);
    }
}
