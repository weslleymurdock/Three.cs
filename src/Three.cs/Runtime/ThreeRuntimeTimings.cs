namespace Three.Runtime;

/// <summary>Durations measured by the JavaScript runtime with <c>performance.now()</c>, in milliseconds.</summary>
/// <param name="ImportMs">Time to fetch and evaluate the Three.js module graph.</param>
/// <param name="InitializeMs">Time to verify the module and detect the environment.</param>
/// <param name="TotalMs">Total JavaScript-side initialization time.</param>
public sealed record ThreeRuntimeTimings(double ImportMs, double InitializeMs, double TotalMs);