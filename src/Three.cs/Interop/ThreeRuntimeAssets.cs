namespace Three.Interop;

/// <summary>
/// Stable Static Web Asset paths of the Three.js runtime. They are identical on every host (Blazor WebAssembly
/// and .NET MAUI Hybrid) because they are resolved against the document base URI.
/// </summary>
public static class ThreeRuntimeAssets
{
    /// <summary>Path of the runtime entry point relative to the host root.</summary>
    public const string ModulePath = "_content/Three.cs/js/three-runtime.js";

    /// <summary>Specifier to pass to the JavaScript <c>import</c> function so it resolves against the document base URI.</summary>
    public const string ModuleSpecifier = "./" + ModulePath;
}