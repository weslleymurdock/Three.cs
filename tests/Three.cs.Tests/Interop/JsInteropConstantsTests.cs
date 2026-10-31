using Three.Interop;

namespace Three.Tests.Interop;

public class JsInteropConstantsTests
{
    [Fact]
    public void Constants_HaveExpectedValues()
    {
        Assert.Equal("./_content/Three.cs/three/three.module.js", JsInteropConstants.ThreeModulePath);
        Assert.Equal("./_content/Three.cs/js/three-bridge.js", JsInteropConstants.BridgeScriptPath);
        Assert.Equal("__threeCsBridge", JsInteropConstants.GlobalBridgeIdentifier);
        Assert.Equal("initialize", JsInteropConstants.InitializeMethod);
        Assert.Equal("dispose", JsInteropConstants.DisposeMethod);
    }
}

