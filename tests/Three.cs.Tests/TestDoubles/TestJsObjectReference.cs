using Microsoft.JSInterop;

namespace Three.Tests.TestDoubles;

public sealed class TestJsObjectReference : IJSObjectReference
{
    private readonly List<string> _invocations = new();

    public IReadOnlyList<string> Invocations => _invocations;

    public Func<string, object?[]?, object?>? InvokeHandler { get; set; }

    public Func<string, object?[]?, Task<object?>>? AsyncInvokeHandler { get; set; }

    public Func<string, object?[]?, Exception?>? ErrorHandler { get; set; }

    public Func<Task>? AsyncDisposeHandler { get; set; }

    public bool IsDisposed { get; private set; }

    public bool ThrowOnDispose { get; set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _invocations.Add(identifier);

        var error = ErrorHandler?.Invoke(identifier, args);
        if (error is not null)
        {
            throw error;
        }

        if (AsyncInvokeHandler is not null)
        {
            var result = await AsyncInvokeHandler(identifier, args).ConfigureAwait(false);
            return (TValue)result!;
        }

        if (InvokeHandler is not null)
        {
            var result = InvokeHandler(identifier, args);
            return (TValue)result!;
        }

        return default!;
    }

    public ValueTask InvokeVoidAsync(string identifier, object?[]? args)
    {
        return InvokeVoidAsync(identifier, CancellationToken.None, args);
    }

    public async ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _invocations.Add(identifier);

        var error = ErrorHandler?.Invoke(identifier, args);
        if (error is not null)
        {
            throw error;
        }

        if (AsyncInvokeHandler is not null)
        {
            await AsyncInvokeHandler(identifier, args).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        IsDisposed = true;
        if (ThrowOnDispose)
        {
            throw new InvalidOperationException("Simulated module disposal failure.");
        }

        if (AsyncDisposeHandler is not null)
        {
            await AsyncDisposeHandler().ConfigureAwait(false);
        }
    }
}
