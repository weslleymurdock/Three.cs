using Three.Hosting;
using Three.Interop;

namespace Three.Tests.Hosting;

public class ThreeOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveExpectedValues()
    {
        var options = new ThreeOptions();

        Assert.Equal(JsInteropConstants.BridgeScriptPath, options.ScriptPath);
        Assert.Equal(JsInteropConstants.ThreeModulePath, options.ThreeModulePath);
        Assert.Equal("three-canvas-", options.CanvasIdPrefix);
        Assert.False(options.EnableDebugDiagnostics);

        options.Validate();
    }

    [Fact]
    public void CustomOptions_CanBeConfiguredAndValidated()
    {
        var options = new ThreeOptions
        {
            ScriptPath = "./my-bridge.js",
            ThreeModulePath = "./three.js",
            CanvasIdPrefix = "viewport-",
            EnableDebugDiagnostics = true
        };

        Assert.Equal("./my-bridge.js", options.ScriptPath);
        Assert.Equal("./three.js", options.ThreeModulePath);
        Assert.Equal("viewport-", options.CanvasIdPrefix);
        Assert.True(options.EnableDebugDiagnostics);

        options.Validate();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidScriptPath_ThrowsInvalidOperationException(string? invalidValue)
    {
        var options = new ThreeOptions { ScriptPath = invalidValue! };
        var ex = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Contains("ScriptPath", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidThreeModulePath_ThrowsInvalidOperationException(string? invalidValue)
    {
        var options = new ThreeOptions { ThreeModulePath = invalidValue! };
        var ex = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Contains("ThreeModulePath", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidCanvasIdPrefix_ThrowsInvalidOperationException(string? invalidValue)
    {
        var options = new ThreeOptions { CanvasIdPrefix = invalidValue! };
        var ex = Assert.Throws<InvalidOperationException>(() => options.Validate());
        Assert.Contains("CanvasIdPrefix", ex.Message);
    }
}
