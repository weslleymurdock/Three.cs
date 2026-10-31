using Microsoft.Extensions.Options;
using Three.Hosting;
using Three.Interop;
using Three.Lifecycle;

namespace Three.Runtime;

/// <summary>
/// Core implementation of <see cref="IThreeRuntime"/> coordinating bridge readiness and lifecycle.
/// </summary>
public sealed class ThreeRuntime : IThreeRuntime
{
    private readonly IJsBridge _bridge;
    private readonly ThreeOptions _options;
    private readonly ThreeLifecycle _lifecycle;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreeRuntime"/> class.
    /// </summary>
    /// <param name="bridge">The JavaScript bridge instance.</param>
    /// <param name="options">Optional hosting configuration options.</param>
    public ThreeRuntime(IJsBridge bridge, IOptions<ThreeOptions>? options = null)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        _options = options?.Value ?? new ThreeOptions();
        _options.Validate();

        _lifecycle = new ThreeLifecycle(
            componentName: nameof(ThreeRuntime),
            onInitialize: async ct =>
            {
                await _bridge.EnsureInitializedAsync(ct).ConfigureAwait(false);
            },
            onDispose: async () =>
            {
                await _bridge.DisposeAsync().ConfigureAwait(false);
            });
    }

    /// <inheritdoc />
    public ThreeLifecycleState State => _lifecycle.State;

    /// <inheritdoc />
    public IJsBridge Bridge => _bridge;

    /// <inheritdoc />
    public ThreeOptions Options => _options;

    /// <inheritdoc />
    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycle.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _lifecycle.DisposeAsync().ConfigureAwait(false);
    }
}

