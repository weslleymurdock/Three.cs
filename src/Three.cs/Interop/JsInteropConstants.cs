namespace Three.Interop;

/// <summary>Central place for the JavaScript identifiers of the runtime contract (<c>three-runtime.js</c>).</summary>
internal static class JsInteropConstants
{
    /// <summary>The identifier of the JavaScript dynamic import function exposed by the JS interop layer.</summary>
    public const string ImportFunction = "import";

    /// <summary>Export that loads and verifies Three.js.</summary>
    public const string Initialize = "initialize";

    /// <summary>Export that returns the upstream provenance.</summary>
    public const string GetUpstreamInfo = "getUpstreamInfo";

    /// <summary>Export that returns the verified capabilities.</summary>
    public const string GetCapabilities = "getCapabilities";

    /// <summary>Export that releases runtime references.</summary>
    public const string Dispose = "dispose";
}