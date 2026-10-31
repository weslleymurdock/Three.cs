using Microsoft.JSInterop;

namespace Three.Tests;

/// <summary>
/// Controllable <see cref="IJSObjectReference"/> for testing <see cref="Three.Runtime.ThreeRuntime"/>.
/// </summary>
public sealed class ScriptedJsObjectReference : IJSObjectReference
{
    private readonly Func<string, object?[], object?> _handler;

    public ScriptedJsObjectReference(Func<string, object?[], object?> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public List<(string Identifier, object?[]? Args)> Invocations { get; } = [];
    public bool Disposed { get; private set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        Invocations.Add((identifier, args));
        var result = _handler(identifier, args ?? []);
        if (result is null || !typeof(TValue).IsAssignableFrom(result.GetType()))
        {
            return ValueTask.FromResult(default(TValue)!);
        }
        return ValueTask.FromResult((TValue)result);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask InvokeVoidAsync(string identifier, object?[]? args)
    {
        Invocations.Add((identifier, args));
        _handler(identifier, args ?? []);
        return ValueTask.CompletedTask;
    }

    public ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return InvokeVoidAsync(identifier, args);
    }

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }
}
