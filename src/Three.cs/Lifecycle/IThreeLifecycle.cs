namespace Three.Lifecycle;

/// <summary>
/// Defines the lifecycle management contract for Three.cs components and runtime instances.
/// </summary>
public interface IThreeLifecycle : IAsyncDisposable
{
    /// <summary>
    /// Gets the current lifecycle state.
    /// </summary>
    ThreeLifecycleState State { get; }

    /// <summary>
    /// Gets a value indicating whether the component is fully initialized and operational.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets a value indicating whether the component has completed disposal.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Initializes the component asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}
