using Three.Runtime;

namespace Three.Interop;

/// <summary>Payload returned by the JavaScript <c>initialize</c> export.</summary>
/// <param name="Ready">Whether the runtime is ready.</param>
/// <param name="Status">JavaScript lifecycle state.</param>
/// <param name="Upstream">Upstream provenance.</param>
/// <param name="Capabilities">Verified capabilities, when ready.</param>
/// <param name="Timings">JavaScript-side durations, when available.</param>
/// <param name="Error">Failure description, when not ready.</param>
internal sealed record ThreeRuntimeInitializeResponse(
    bool Ready,
    string Status,
    ThreeUpstreamInfo Upstream,
    ThreeRuntimeCapabilities? Capabilities,
    ThreeRuntimeTimings? Timings,
    ThreeRuntimeErrorInfo? Error);