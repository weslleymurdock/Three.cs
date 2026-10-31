namespace Three.Validation.Maui.Models;

/// <summary>
/// Aggregated validation report containing environment details and iteration metrics.
/// </summary>
public sealed record MauiValidationReport(
    MauiEnvironmentInfo Environment,
    double NativeWebViewReadyMs,
    double BlazorComponentReadyMs,
    IReadOnlyList<MauiIterationMetric> Iterations,
    MauiTimingStats WarmOperationStats,
    int ErrorCount);
