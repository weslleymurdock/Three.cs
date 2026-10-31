namespace Three.Runtime;

/// <summary>Capability groups that were effectively verified against the loaded Three.js build.</summary>
/// <param name="Available">Names of the available capability groups (for example <c>core</c>, <c>scene-graph</c>, <c>renderer-webgl</c>).</param>
/// <param name="Unavailable">Names of the capability groups the loaded build does not provide.</param>
/// <param name="Environment">Features detected in the current environment.</param>
public sealed record ThreeRuntimeCapabilities(
    IReadOnlyList<string> Available,
    IReadOnlyList<string> Unavailable,
    ThreeRuntimeEnvironment Environment);