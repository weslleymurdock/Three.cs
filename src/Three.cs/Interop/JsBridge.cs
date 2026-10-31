using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Three.Hosting;

namespace Three.Interop;

/// <summary>
/// Default implementation of <see cref="IJsBridge"/> communicating with the browser or WebView via <see cref="IJSRuntime"/>.
/// </summary>
public sealed class JsBridge : IJsBridge
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ThreeOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IJSObjectReference? _moduleReference;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsBridge"/> class.
    /// </summary>
    /// <param name="jsRuntime">The underlying JavaScript runtime.</param>
    /// <param name="options">Optional hosting options for script paths.</param>
    public JsBridge(IJSRuntime jsRuntime, IOptions<ThreeOptions>? options = null)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _options = options?.Value ?? new ThreeOptions();
    }

    /// <inheritdoc />
    public bool IsReady => !_isDisposed && _moduleReference is not null;

    /// <inheritdoc />
    public async ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (IsReady)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();

            if (_moduleReference is not null)
            {
                return;
            }

            try
            {
                _moduleReference = await _jsRuntime
                    .InvokeAsync<IJSObjectReference>("import", cancellationToken, _options.ScriptPath)
                    .ConfigureAwait(false);

                await _moduleReference
                    .InvokeVoidAsync(JsInteropConstants.InitializeMethod, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (JSException jsEx)
            {
                throw new JsBridgeException(
                    $"Failed to initialize Three.cs JavaScript bridge from '{_options.ScriptPath}'. Ensure static web assets are correctly mapped. Details: {jsEx.Message}",
                    jsEx);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new JsBridgeException(
                    $"An unexpected error occurred while initializing the Three.cs JavaScript bridge from '{_options.ScriptPath}'.",
                    ex);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken = default, params object?[]? args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        ThrowIfDisposed();
        EnsureBridgeReady();

        try
        {
            await _moduleReference!
                .InvokeVoidAsync(identifier, cancellationToken, args ?? Array.Empty<object?>())
                .ConfigureAwait(false);
        }
        catch (JSException jsEx)
        {
            throw new JsBridgeException(
                $"Failed to invoke JavaScript function '{identifier}' on Three.cs bridge: {jsEx.Message}",
                jsEx);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new JsBridgeException(
                $"An unexpected error occurred while invoking '{identifier}' on Three.cs bridge.",
                ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<TResult> InvokeAsync<TResult>(string identifier, CancellationToken cancellationToken = default, params object?[]? args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        ThrowIfDisposed();
        EnsureBridgeReady();

        try
        {
            return await _moduleReference!
                .InvokeAsync<TResult>(identifier, cancellationToken, args ?? Array.Empty<object?>())
                .ConfigureAwait(false);
        }
        catch (JSException jsEx)
        {
            throw new JsBridgeException(
                $"Failed to invoke JavaScript function '{identifier}' with return type '{typeof(TResult).Name}' on Three.cs bridge: {jsEx.Message}",
                jsEx);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new JsBridgeException(
                $"An unexpected error occurred while invoking '{identifier}' with return type '{typeof(TResult).Name}' on Three.cs bridge.",
                ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_moduleReference is not null)
            {
                try
                {
                    await _moduleReference.InvokeVoidAsync(JsInteropConstants.DisposeMethod).ConfigureAwait(false);
                }
                catch
                {
                    // Swallowing disposal invocation errors during shutdown
                }

                try
                {
                    await _moduleReference.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Suppress secondary module disposal errors
                }
                finally
                {
                    _moduleReference = null;
                }
            }
        }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(JsBridge), "The JavaScript bridge has been disposed.");
        }
    }

    private void EnsureBridgeReady()
    {
        if (!IsReady)
        {
            throw new JsBridgeException("The JavaScript bridge is not ready. Call EnsureInitializedAsync() before invoking bridge functions.");
        }
    }
}
