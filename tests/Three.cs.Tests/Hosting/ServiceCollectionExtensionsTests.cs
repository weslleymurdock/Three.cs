using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Three.Hosting;
using Three.Interop;
using Three.Runtime;
using Three.Tests.TestDoubles;

namespace Three.Tests.Hosting;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddThree_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddThree(null!));
    }

    [Fact]
    public void AddThree_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJSRuntime, TestJsRuntime>();

        services.AddThree();

        var provider = services.BuildServiceProvider();

        var bridge = provider.GetService<IJsBridge>();
        var runtime = provider.GetService<IThreeRuntime>();
        var options = provider.GetService<IOptions<ThreeOptions>>();

        Assert.NotNull(bridge);
        Assert.IsType<JsBridge>(bridge);

        Assert.NotNull(runtime);
        Assert.IsType<ThreeRuntime>(runtime);

        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddThree_WithConfigure_AppliesConfiguration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJSRuntime, TestJsRuntime>();

        services.AddThree(options =>
        {
            options.CanvasIdPrefix = "custom-prefix-";
            options.EnableDebugDiagnostics = true;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ThreeOptions>>().Value;

        Assert.Equal("custom-prefix-", options.CanvasIdPrefix);
        Assert.True(options.EnableDebugDiagnostics);
    }
}
