namespace Three.Interop;

/// <summary>
/// Abstraction for communicating across JavaScript runtime and WebView hosts.
/// </summary>
public interface IJsBridge : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether the bridge is ready for invocations.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Ensures that the JavaScript bridge and runtime module are loaded and ready.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the initialization operation.</returns>
    ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a JavaScript function asynchronously without returning a value.
    /// </summary>
    /// <param name="identifier">Identifier of the function to invoke.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <param name="args">Arguments to pass to the function.</param>
    /// <returns>A task representing the invocation.</returns>
    ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken = default, params object?[]? args);

    /// <summary>
    /// Invokes a JavaScript function asynchronously returning a deserialized result.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="identifier">Identifier of the function to invoke.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <param name="args">Arguments to pass to the function.</param>
    /// <returns>A task returning the invocation result.</returns>
    ValueTask<TResult> InvokeAsync<TResult>(string identifier, CancellationToken cancellationToken = default, params object?[]? args);
}
