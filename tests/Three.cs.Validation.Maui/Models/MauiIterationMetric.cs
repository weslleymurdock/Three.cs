namespace Three.Validation.Maui.Models;

/// <summary>
/// Detailed measurements for a single validation run iteration.
/// </summary>
public sealed record MauiIterationMetric(
    int Iteration,
    string Operation,
    bool IsCold,
    double ModuleImportMs,
    double RuntimeInitMs,
    double TotalMs,
    double JsImportMs,
    double JsInitMs,
    double JsTotalMs,
    string? Error);
