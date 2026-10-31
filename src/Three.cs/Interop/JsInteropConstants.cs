namespace Three.Interop;

/// <summary>
/// Centralizes JavaScript module paths, script identifiers, and bridge function names.
/// </summary>
public static class JsInteropConstants
{
    /// <summary>
    /// Path to the Three.js ES module bundle hosted by the Razor Class Library.
    /// </summary>
    public const string ThreeModulePath = "./_content/Three.cs/three/three.module.js";

    /// <summary>
    /// Path to the Three.cs JavaScript bridge script.
    /// </summary>
    public const string BridgeScriptPath = "./_content/Three.cs/js/three-bridge.js";

    /// <summary>
    /// Global window property name where the Three.cs bridge is registered.
    /// </summary>
    public const string GlobalBridgeIdentifier = "__threeCsBridge";

    /// <summary>
    /// JavaScript function name to initialize the bridge.
    /// </summary>
    public const string InitializeMethod = "initialize";

    /// <summary>
    /// JavaScript function name to dispose the bridge.
    /// </summary>
    public const string DisposeMethod = "dispose";
}

