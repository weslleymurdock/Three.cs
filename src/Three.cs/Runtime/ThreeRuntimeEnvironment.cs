namespace Three.Runtime;

/// <summary>Environment features detected by the runtime in the current WebView or browser.</summary>
/// <param name="WebGL2">Whether a WebGL2 context can be created.</param>
public sealed record ThreeRuntimeEnvironment(bool WebGL2);