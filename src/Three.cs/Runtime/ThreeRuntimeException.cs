namespace Three.Runtime;

/// <summary>Raised when the Three.js runtime cannot be loaded, initialized or queried.</summary>
public sealed class ThreeRuntimeException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="ThreeRuntimeException"/> class.</summary>
    /// <param name="code">Stable failure code. See <see cref="ThreeRuntimeFailureCodes"/>.</param>
    /// <param name="stage">Stage in which the failure happened (for example <c>import</c> or <c>initialize</c>).</param>
    /// <param name="message">Actionable description of the failure.</param>
    /// <param name="innerException">The underlying exception, if any.</param>
    public ThreeRuntimeException(string code, string stage, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Stage = stage;
    }

    /// <summary>Gets the stable failure code. See <see cref="ThreeRuntimeFailureCodes"/>.</summary>
    public string Code { get; }

    /// <summary>Gets the stage in which the failure happened.</summary>
    public string Stage { get; }
}