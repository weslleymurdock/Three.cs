namespace Three.Validation.Maui.Models;

/// <summary>
/// Aggregated validation report containing environment details and iteration metrics.
/// </summary>
public sealed record MauiValidationReport(
    MauiEnvironmentInfo Environment,
    double WebViewReadyMs,
    IReadOnlyList<MauiIterationMetric> Iterations,
    MauiTimingStats WarmTotalStats,
    int ErrorCount);

