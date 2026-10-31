namespace Three.Validation.Maui.Models;

/// <summary>
/// Execution environment information collected on the host.
/// </summary>
public sealed record MauiEnvironmentInfo(
    string Platform,
    string OperatingSystem,
    string DeviceModel,
    string WebViewUserAgent,
    string BuildConfiguration);

