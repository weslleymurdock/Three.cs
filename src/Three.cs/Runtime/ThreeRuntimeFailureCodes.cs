namespace Three.Runtime;

/// <summary>Stable failure codes carried by <see cref="ThreeRuntimeException.Code"/>.</summary>
/// <remarks>
/// Codes <see cref="ImportFailed"/>, <see cref="IncompatibleRuntime"/> and <see cref="InitializationFailed"/>
/// are produced by the JavaScript runtime. The remaining codes are produced by the C# integration.
/// </remarks>
public static class ThreeRuntimeFailureCodes
{
    /// <summary>The Three.js module or one of its dependencies could not be fetched or evaluated.</summary>
    public const string ImportFailed = "import-failed";

    /// <summary>The module loaded but does not provide the symbols required by the contract.</summary>
    public const string IncompatibleRuntime = "incompatible-runtime";

    /// <summary>The module loaded but its post-load verification failed.</summary>
    public const string InitializationFailed = "initialization-failed";

    /// <summary>The runtime entry point module could not be imported from the Static Web Assets.</summary>
    public const string ModuleImportFailed = "module-import-failed";

    /// <summary>A JavaScript call failed with an exception.</summary>
    public const string JavaScriptError = "js-error";

    /// <summary>The JavaScript runtime answered with a payload that violates the contract.</summary>
    public const string InvalidResponse = "invalid-response";

    /// <summary>A query that requires a ready runtime was made in another state.</summary>
    public const string NotInitialized = "not-initialized";
}