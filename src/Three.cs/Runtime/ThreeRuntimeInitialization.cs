namespace Three.Runtime;

/// <summary>Outcome of a successful runtime initialization.</summary>
/// <param name="Upstream">Provenance of the loaded Three.js build.</param>
/// <param name="Capabilities">Verified capabilities.</param>
/// <param name="Timings">JavaScript-side durations.</param>
public sealed record ThreeRuntimeInitialization(
    ThreeUpstreamInfo Upstream,
    ThreeRuntimeCapabilities Capabilities,
    ThreeRuntimeTimings Timings);