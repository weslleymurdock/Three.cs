using Three.Interop;
using Three.Runtime;

namespace Three.Tests;

public class ThreeRuntimeModelTests
{
    [Fact]
    public void ThreeUpstreamInfo_SupportsEqualityAndRecordFeatures()
    {
        var info1 = new ThreeUpstreamInfo(
            Name: "three",
            Version: "0.187.0",
            Revision: "187",
            Commit: "cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168",
            License: "MIT",
            Repository: "https://github.com/mrdoob/three.js",
            LoadedRevision: "187");

        var info2 = new ThreeUpstreamInfo(
            Name: "three",
            Version: "0.187.0",
            Revision: "187",
            Commit: "cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168",
            License: "MIT",
            Repository: "https://github.com/mrdoob/three.js",
            LoadedRevision: "187");

        Assert.Equal(info1, info2);
        Assert.True(info1 == info2);
        Assert.False(info1 != info2);
        Assert.Equal(info1.GetHashCode(), info2.GetHashCode());
        Assert.Contains("three", info1.ToString());

        var cloned = info1 with { LoadedRevision = "188" };
        Assert.NotEqual(info1, cloned);

        var (name, version, revision, commit, license, repo, loaded) = info1;
        Assert.Equal("three", name);
        Assert.Equal("0.187.0", version);
        Assert.Equal("187", revision);
        Assert.Equal("cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168", commit);
        Assert.Equal("MIT", license);
        Assert.Equal("https://github.com/mrdoob/three.js", repo);
        Assert.Equal("187", loaded);
    }

    [Fact]
    public void ThreeRuntimeCapabilities_SupportsEqualityAndRecordFeatures()
    {
        var env1 = new ThreeRuntimeEnvironment(WebGL2: true);
        var env2 = new ThreeRuntimeEnvironment(WebGL2: true);
        Assert.Equal(env1, env2);
        Assert.True(env1 == env2);
        Assert.Equal(env1.GetHashCode(), env2.GetHashCode());
        Assert.Contains("True", env1.ToString());

        var caps1 = new ThreeRuntimeCapabilities(
            Available: ["core", "lights"],
            Unavailable: ["webgpu"],
            Environment: env1);

        var caps2 = caps1 with { Available = ["core", "lights"] };
        Assert.Contains("ThreeRuntimeCapabilities", caps1.ToString());
        Assert.Equal(env1, caps1.Environment);

        var (avail, unavail, env) = caps1;
        Assert.Equal(2, avail.Count);
        Assert.Single(unavail);
        Assert.True(env.WebGL2);
    }

    [Fact]
    public void ThreeRuntimeTimings_SupportsEqualityAndRecordFeatures()
    {
        var t1 = new ThreeRuntimeTimings(10.0, 5.0, 15.0);
        var t2 = new ThreeRuntimeTimings(10.0, 5.0, 15.0);

        Assert.Equal(t1, t2);
        Assert.True(t1 == t2);
        Assert.False(t1 != t2);
        Assert.Equal(t1.GetHashCode(), t2.GetHashCode());
        Assert.Contains("10", t1.ToString());

        var (imp, init, tot) = t1;
        Assert.Equal(10.0, imp);
        Assert.Equal(5.0, init);
        Assert.Equal(15.0, tot);
    }

    [Fact]
    public void ThreeRuntimeInitialization_SupportsEqualityAndRecordFeatures()
    {
        var upstream = new ThreeUpstreamInfo("three", "0.187.0", "187", "c", "MIT", "r", "187");
        var caps = new ThreeRuntimeCapabilities(["core"], [], new ThreeRuntimeEnvironment(true));
        var timings = new ThreeRuntimeTimings(1, 2, 3);

        var init1 = new ThreeRuntimeInitialization(upstream, caps, timings);
        var init2 = new ThreeRuntimeInitialization(upstream, caps, timings);

        Assert.Equal(init1, init2);
        Assert.True(init1 == init2);
        Assert.Equal(init1.GetHashCode(), init2.GetHashCode());
        Assert.Contains("three", init1.ToString());

        var (u, c, t) = init1;
        Assert.Equal(upstream, u);
        Assert.Equal(caps, c);
        Assert.Equal(timings, t);
    }

    [Fact]
    public void ThreeRuntimeErrorInfo_SupportsEqualityAndRecordFeatures()
    {
        var err1 = new ThreeRuntimeErrorInfo("code", "stage", "msg");
        var err2 = new ThreeRuntimeErrorInfo("code", "stage", "msg");

        Assert.Equal(err1, err2);
        Assert.True(err1 == err2);
        Assert.Equal(err1.GetHashCode(), err2.GetHashCode());
        Assert.Contains("code", err1.ToString());

        var (code, stage, msg) = err1;
        Assert.Equal("code", code);
        Assert.Equal("stage", stage);
        Assert.Equal("msg", msg);
    }

    [Fact]
    public void ThreeRuntimeInitializeResponse_SupportsEqualityAndRecordFeatures()
    {
        var upstream = new ThreeUpstreamInfo("three", "0.187.0", "187", "c", "MIT", "r", "187");
        var resp1 = new ThreeRuntimeInitializeResponse(true, "ready", upstream, null, null, null);
        var resp2 = new ThreeRuntimeInitializeResponse(true, "ready", upstream, null, null, null);

        Assert.Equal(resp1, resp2);
        Assert.True(resp1 == resp2);
        Assert.Equal(resp1.GetHashCode(), resp2.GetHashCode());
        Assert.Contains("ready", resp1.ToString());

        var (ready, status, u, c, t, err) = resp1;
        Assert.True(ready);
        Assert.Equal("ready", status);
        Assert.Equal(upstream, u);
        Assert.Null(c);
        Assert.Null(t);
        Assert.Null(err);
    }

    [Fact]
    public void ThreeRuntimeException_CarriesCodeAndStage()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ThreeRuntimeException("test-code", "test-stage", "test-msg", inner);

        Assert.Equal("test-code", ex.Code);
        Assert.Equal("test-stage", ex.Stage);
        Assert.Equal("test-msg", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void FailureCodes_ExposeExpectedConstants()
    {
        Assert.Equal("import-failed", ThreeRuntimeFailureCodes.ImportFailed);
        Assert.Equal("incompatible-runtime", ThreeRuntimeFailureCodes.IncompatibleRuntime);
        Assert.Equal("initialization-failed", ThreeRuntimeFailureCodes.InitializationFailed);
        Assert.Equal("module-import-failed", ThreeRuntimeFailureCodes.ModuleImportFailed);
        Assert.Equal("js-error", ThreeRuntimeFailureCodes.JavaScriptError);
        Assert.Equal("invalid-response", ThreeRuntimeFailureCodes.InvalidResponse);
        Assert.Equal("not-initialized", ThreeRuntimeFailureCodes.NotInitialized);
    }
}
