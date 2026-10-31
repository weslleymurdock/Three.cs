namespace Three.Interop;

/// <summary>Failure description produced by the JavaScript runtime.</summary>
/// <param name="Code">Stable failure code.</param>
/// <param name="Stage">Failure stage.</param>
/// <param name="Message">Description.</param>
internal sealed record ThreeRuntimeErrorInfo(string Code, string Stage, string Message);