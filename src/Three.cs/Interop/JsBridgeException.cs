namespace Three.Interop;

/// <summary>
/// Exception thrown when a JavaScript interop or bridge failure occurs.
/// </summary>
public class JsBridgeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsBridgeException"/> class.
    /// </summary>
    /// <param name="message">The actionable diagnostic message describing the failure.</param>
    public JsBridgeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsBridgeException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The actionable diagnostic message describing the failure.</param>
    /// <param name="innerException">The underlying exception that caused the bridge failure.</param>
    public JsBridgeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
