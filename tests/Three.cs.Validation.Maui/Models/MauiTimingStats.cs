namespace Three.Validation.Maui.Models;

/// <summary>
/// Statistical summary (mean, median, min, max) for a set of measurements.
/// </summary>
public sealed record MauiTimingStats(
    double Mean,
    double Median,
    double Min,
    double Max);

