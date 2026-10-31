using System.Diagnostics;

namespace Three.Tests;

public class NodeJsRuntimeTests
{
    [Fact]
    public void JavaScriptTestSuite_PassesInNodeJs()
    {
        var solutionRoot = SolutionPathHelper.SolutionRoot;
        var testScriptPath = Path.Combine(solutionRoot, "tests", "js", "three-runtime.test.js");
        Assert.True(File.Exists(testScriptPath), $"Expected JS test file at: {testScriptPath}");

        var psi = new ProcessStartInfo("node", $"--test \"{testScriptPath}\"")
        {
            WorkingDirectory = solutionRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit(20000);
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        Assert.True(process.ExitCode == 0, $"Node JS test suite failed (exit {process.ExitCode}):\n{stdout}\n{stderr}");
    }
}

