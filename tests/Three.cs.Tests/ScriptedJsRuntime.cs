using Microsoft.JSInterop;

namespace Three.Tests;

/// <summary>
/// Controllable <see cref="IJSRuntime"/> for testing <see cref="Three.Runtime.ThreeRuntime"/>.
/// </summary>
public sealed class ScriptedJsRuntime : IJSRuntime
{
    private readonly Func<string, object?[], object?> _handler;

    public ScriptedJsRuntime(Func<string, object?[], object?> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public List<(string Identifier, object?[]? Args)> Invocations { get; } = [];

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        Invocations.Add((identifier, args));
        var result = _handler(identifier, args ?? []);
        return ValueTask.FromResult((TValue)result!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return InvokeAsync<TValue>(identifier, args);
    }
}

