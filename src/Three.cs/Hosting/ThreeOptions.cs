using Three.Interop;

namespace Three.Hosting;

/// <summary>
/// Configuration options for Three.cs hosting, runtime, and JavaScript interop.
/// </summary>
public sealed class ThreeOptions
{
    /// <summary>
    /// Gets or sets the path to the bridge script.
    /// </summary>
    public string ScriptPath { get; set; } = JsInteropConstants.BridgeScriptPath;

    /// <summary>
    /// Gets or sets the path to the Three.js ES module bundle.
    /// </summary>
    public string ThreeModulePath { get; set; } = JsInteropConstants.ThreeModulePath;

    /// <summary>
    /// Gets or sets the prefix used when generating unique canvas element IDs.
    /// </summary>
    public string CanvasIdPrefix { get; set; } = "three-canvas-";

    /// <summary>
    /// Gets or sets a value indicating whether debug diagnostics and verbose interop logging are enabled.
    /// </summary>
    public bool EnableDebugDiagnostics { get; set; }

    /// <summary>
    /// Validates the current option values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required options are invalid or missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ScriptPath))
        {
            throw new InvalidOperationException("ThreeOptions.ScriptPath must be configured with a valid non-empty path.");
        }

        if (string.IsNullOrWhiteSpace(ThreeModulePath))
        {
            throw new InvalidOperationException("ThreeOptions.ThreeModulePath must be configured with a valid non-empty path.");
        }

        if (string.IsNullOrWhiteSpace(CanvasIdPrefix))
        {
            throw new InvalidOperationException("ThreeOptions.CanvasIdPrefix must be configured with a valid non-empty prefix.");
        }
    }
}
