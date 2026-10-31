namespace Three.Runtime;

/// <summary>
/// Loads, initializes and releases the Three.js runtime shipped by the Three.cs Razor Class Library.
/// A runtime instance owns the JavaScript module reference and must be used by a single owner:
/// the JavaScript runtime is process-wide per document, so disposing it releases it for every consumer.
/// </summary>
public interface IThreeRuntime : IAsyncDisposable
{
    /// <summary>Gets the current lifecycle state.</summary>
    ThreeRuntimeState State { get; }

    /// <summary>
    /// Pre-imports the JavaScript runtime entry point module from the RCL Static Web Assets.
    /// Calling this before <see cref="InitializeAsync"/> allows measuring module import duration separately
    /// from runtime verification. If not called explicitly, <see cref="InitializeAsync"/> imports the module automatically.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <exception cref="ThreeRuntimeException">Importing the module from Static Web Assets failed.</exception>
    /// <exception cref="OperationCanceledException">The operation was cancelled.</exception>
    /// <exception cref="ObjectDisposedException">The runtime was disposed.</exception>
    Task ImportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports the runtime module and initializes Three.js. Concurrent calls are serialized and calling it
    /// again once <see cref="ThreeRuntimeState.Ready"/> returns the same result without reloading.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation. Cancellation never marks the runtime as failed.</param>
    /// <returns>The verified runtime information.</returns>
    /// <exception cref="ThreeRuntimeException">Loading or initialization failed. The state becomes <see cref="ThreeRuntimeState.Failed"/> and a new attempt is allowed.</exception>
    /// <exception cref="OperationCanceledException">The operation was cancelled or the runtime was disposed in flight.</exception>
    /// <exception cref="ObjectDisposedException">The runtime was disposed.</exception>
    Task<ThreeRuntimeInitialization> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Queries the upstream provenance from the ready runtime.</summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <exception cref="ThreeRuntimeException">The runtime is not ready, or the query failed.</exception>
    /// <exception cref="ObjectDisposedException">The runtime was disposed.</exception>
    Task<ThreeUpstreamInfo> GetUpstreamInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>Queries the verified capabilities from the ready runtime.</summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <exception cref="ThreeRuntimeException">The runtime is not ready, or the query failed.</exception>
    /// <exception cref="ObjectDisposedException">The runtime was disposed.</exception>
    Task<ThreeRuntimeCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default);
}