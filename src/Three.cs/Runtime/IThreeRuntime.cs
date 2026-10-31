using Three.Hosting;
using Three.Interop;
using Three.Lifecycle;

namespace Three.Runtime;

/// <summary>
/// Represents an active Three.cs runtime instance coordinating lifecycle, hosting options, and JavaScript bridge.
/// </summary>
public interface IThreeRuntime : IAsyncDisposable
{
    /// <summary>
    /// Gets the current lifecycle state of the runtime.
    /// </summary>
    ThreeLifecycleState State { get; }

    /// <summary>
    /// Gets the underlying JavaScript interop bridge.
    /// </summary>
    IJsBridge Bridge { get; }

    /// <summary>
    /// Gets the configured hosting options.
    /// </summary>
    ThreeOptions Options { get; }

    /// <summary>
    /// Initializes the Three.cs runtime and ensures the bridge is ready.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}
