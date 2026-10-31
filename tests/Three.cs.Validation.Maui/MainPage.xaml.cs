using System.Diagnostics;
using Microsoft.AspNetCore.Components.WebView;

namespace Three.Validation.Maui;

public partial class MainPage : ContentPage
{
    public static double NativeWebViewReadyMs { get; private set; } = -1;

    public MainPage()
    {
        InitializeComponent();
        blazorWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
    }

    private void OnBlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
        NativeWebViewReadyMs = Stopwatch.GetElapsedTime(MauiProgram.AppStartTimestamp).TotalMilliseconds;
    }
}
