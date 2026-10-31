namespace Three.Lifecycle;

/// <summary>
/// Thread-safe lifecycle controller managing initialization, state transitions, and disposal.
/// </summary>
public sealed class ThreeLifecycle : IThreeLifecycle
{
    private readonly Func<CancellationToken, ValueTask>? _onInitialize;
    private readonly Func<ValueTask>? _onDispose;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _componentName;
    private ThreeLifecycleState _state = ThreeLifecycleState.Uninitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreeLifecycle"/> class.
    /// </summary>
    /// <param name="componentName">An optional descriptive name for diagnostics.</param>
    /// <param name="onInitialize">An optional asynchronous initialization action.</param>
    /// <param name="onDispose">An optional asynchronous disposal action.</param>
    public ThreeLifecycle(
        string? componentName = null,
        Func<CancellationToken, ValueTask>? onInitialize = null,
        Func<ValueTask>? onDispose = null)
    {
        _componentName = componentName ?? "ThreeComponent";
        _onInitialize = onInitialize;
        _onDispose = onDispose;
    }

    /// <inheritdoc />
    public ThreeLifecycleState State => _state;

    /// <inheritdoc />
    public bool IsInitialized => _state == ThreeLifecycleState.Initialized;

    /// <inheritdoc />
    public bool IsDisposed => _state == ThreeLifecycleState.Disposed;

    /// <summary>
    /// Ensures that the component is initialized and has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the component is disposed or disposing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the component is uninitialized or initializing.</exception>
    public void ThrowIfNotInitialized()
    {
        if (_state is ThreeLifecycleState.Disposed or ThreeLifecycleState.Disposing)
        {
            throw new ObjectDisposedException(_componentName, "The component has been disposed.");
        }

        if (_state != ThreeLifecycleState.Initialized)
        {
            throw new InvalidOperationException($"The component '{_componentName}' is not initialized (current state: {_state}).");
        }
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_state is ThreeLifecycleState.Disposed or ThreeLifecycleState.Disposing)
        {
            throw new ObjectDisposedException(_componentName, "Cannot initialize a disposed component.");
        }

        if (IsInitialized)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_state is ThreeLifecycleState.Disposed or ThreeLifecycleState.Disposing)
            {
                throw new ObjectDisposedException(_componentName, "Cannot initialize a disposed component.");
            }

            if (IsInitialized)
            {
                return;
            }

            _state = ThreeLifecycleState.Initializing;

            if (_onInitialize is not null)
            {
                await _onInitialize(cancellationToken).ConfigureAwait(false);
            }

            _state = ThreeLifecycleState.Initialized;
        }
        catch
        {
            _state = ThreeLifecycleState.Uninitialized;
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_state is ThreeLifecycleState.Disposed or ThreeLifecycleState.Disposing)
        {
            return;
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_state is ThreeLifecycleState.Disposed or ThreeLifecycleState.Disposing)
            {
                return;
            }

            _state = ThreeLifecycleState.Disposing;

            if (_onDispose is not null)
            {
                await _onDispose().ConfigureAwait(false);
            }

            _state = ThreeLifecycleState.Disposed;
        }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}
