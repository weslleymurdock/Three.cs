using System.Text.Json;
using Microsoft.JSInterop;
using Three.Interop;

namespace Three.Runtime;

/// <summary>
/// Default <see cref="IThreeRuntime"/>: imports the runtime entry point from the RCL Static Web Assets
/// (<see cref="ThreeRuntimeAssets.ModuleSpecifier"/>) through <see cref="IJSRuntime"/> and drives its lifecycle.
/// It is host independent: it behaves the same in Blazor WebAssembly and in .NET MAUI Hybrid.
/// </summary>
/// <remarks>
/// Operations are serialized with an asynchronous gate, so concurrent callers never trigger concurrent
/// initialization and nobody blocks a thread. <see cref="DisposeAsync"/> cancels an in-flight initialization.
/// </remarks>
/// <param name="jsRuntime">The JavaScript runtime of the host.</param>
public sealed class ThreeRuntime(IJSRuntime jsRuntime) : IThreeRuntime
{
    private readonly IJSRuntime _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));

    // The gate owns no OS resources (its wait handle is never requested), so it is intentionally not disposed:
    // disposing it would turn late callers' WaitAsync into ObjectDisposedException instead of the documented one.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _lifetime = new();

    private IJSObjectReference? _module;
    private ThreeRuntimeInitialization? _initialization;
    private int _state = (int)ThreeRuntimeState.Uninitialized;
    private int _disposeRequested;

    /// <inheritdoc />
    public ThreeRuntimeState State => (ThreeRuntimeState)Volatile.Read(ref _state);

    /// <inheritdoc />
    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (_module is not null)
            {
                return;
            }

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
            try
            {
                _module = await ImportModuleAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                SetState(ThreeRuntimeState.Failed);
                throw;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ThreeRuntimeInitialization> InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (State == ThreeRuntimeState.Ready)
            {
                return _initialization!;
            }

            var previous = State;
            SetState(ThreeRuntimeState.Initializing);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
            try
            {
                _module ??= await ImportModuleAsync(linked.Token).ConfigureAwait(false);
                var response = await _module
                    .InvokeAsync<ThreeRuntimeInitializeResponse>(JsInteropConstants.Initialize, linked.Token)
                    .ConfigureAwait(false);

                _initialization = ToInitialization(response);
                SetState(ThreeRuntimeState.Ready);
                return _initialization;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is not a failure: restore the state that existed before this attempt.
                SetState(previous);
                throw;
            }
            catch (JSException exception)
            {
                throw new ThreeRuntimeException(
                    ThreeRuntimeFailureCodes.JavaScriptError,
                    "initialize",
                    $"The Three.js runtime initialization threw a JavaScript exception: {exception.Message}",
                    exception);
            }
            catch (JsonException exception)
            {
                throw new ThreeRuntimeException(
                    ThreeRuntimeFailureCodes.InvalidResponse,
                    "initialize",
                    $"The Three.js runtime returned a payload that does not match the contract: {exception.Message}",
                    exception);
            }
            finally
            {
                // Every exit that is not success or cancellation (including unexpected exceptions) is a failed attempt.
                if (State == ThreeRuntimeState.Initializing)
                {
                    SetState(ThreeRuntimeState.Failed);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public Task<ThreeUpstreamInfo> GetUpstreamInfoAsync(CancellationToken cancellationToken = default) =>
        QueryAsync<ThreeUpstreamInfo>(JsInteropConstants.GetUpstreamInfo, cancellationToken);

    /// <inheritdoc />
    public Task<ThreeRuntimeCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default) =>
        QueryAsync<ThreeRuntimeCapabilities>(JsInteropConstants.GetCapabilities, cancellationToken);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeRequested, 1) == 0)
        {
            // Abort an in-flight initialization so that it releases the gate promptly.
            await _lifetime.CancelAsync().ConfigureAwait(false);
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (State == ThreeRuntimeState.Disposed)
            {
                return;
            }

            SetState(ThreeRuntimeState.Disposed);
            var module = _module;
            _module = null;
            _initialization = null;
            _lifetime.Dispose();

            if (module is null)
            {
                return;
            }

            try
            {
                await module.InvokeVoidAsync(JsInteropConstants.Dispose).ConfigureAwait(false);
            }
            catch (JSDisconnectedException)
            {
                // The JavaScript side is already gone (for example a closed circuit): there is nothing left to release.
            }
            finally
            {
                await module.DisposeAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<TResult> QueryAsync<TResult>(string function, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (State != ThreeRuntimeState.Ready || _module is null)
            {
                throw new ThreeRuntimeException(
                    ThreeRuntimeFailureCodes.NotInitialized,
                    "query",
                    $"The Three.js runtime is '{State}'. Call {nameof(IThreeRuntime.InitializeAsync)} and wait for it to complete before querying it.");
            }

            try
            {
                return await _module.InvokeAsync<TResult>(function, cancellationToken).ConfigureAwait(false);
            }
            catch (JSException exception)
            {
                throw new ThreeRuntimeException(
                    ThreeRuntimeFailureCodes.JavaScriptError,
                    "query",
                    $"The Three.js runtime query '{function}' threw a JavaScript exception: {exception.Message}",
                    exception);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IJSObjectReference> ImportModuleAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _jsRuntime
                .InvokeAsync<IJSObjectReference>(JsInteropConstants.ImportFunction, cancellationToken, ThreeRuntimeAssets.ModuleSpecifier)
                .ConfigureAwait(false);
        }
        catch (JSException exception)
        {
            throw new ThreeRuntimeException(
                ThreeRuntimeFailureCodes.ModuleImportFailed,
                "import",
                $"The Three.js runtime entry point '{ThreeRuntimeAssets.ModuleSpecifier}' could not be imported. Verify that the host serves the Three.cs Static Web Assets under '_content/Three.cs/'. {exception.Message}",
                exception);
        }
    }

    private static ThreeRuntimeInitialization ToInitialization(ThreeRuntimeInitializeResponse? response)
    {
        if (response is null)
        {
            throw new ThreeRuntimeException(
                ThreeRuntimeFailureCodes.InvalidResponse,
                "initialize",
                "The Three.js runtime returned no initialization result.");
        }

        if (!response.Ready)
        {
            var error = response.Error;
            throw new ThreeRuntimeException(
                error?.Code ?? ThreeRuntimeFailureCodes.InvalidResponse,
                error?.Stage ?? "initialize",
                error?.Message ?? $"The Three.js runtime reported state '{response.Status}' without an error description.");
        }

        if (response.Capabilities is null || response.Timings is null)
        {
            throw new ThreeRuntimeException(
                ThreeRuntimeFailureCodes.InvalidResponse,
                "initialize",
                "The Three.js runtime reported ready without capabilities or timings.");
        }

        return new ThreeRuntimeInitialization(response.Upstream, response.Capabilities, response.Timings);
    }

    private void SetState(ThreeRuntimeState state) => Volatile.Write(ref _state, (int)state);

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeRequested) == 1 || State == ThreeRuntimeState.Disposed, this);
}
