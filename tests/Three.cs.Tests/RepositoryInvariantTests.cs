using System.Text.Json;

namespace Three.Tests;

public class RepositoryInvariantTests
{
    [Fact]
    public void ToolManifest_IsRootAndPinsLocalReportGenerator()
    {
        var manifestPath = Path.Combine(SolutionPathHelper.SolutionRoot, ".config", "dotnet-tools.json");
        Assert.True(File.Exists(manifestPath), $"Expected local tool manifest at '{manifestPath}'.");

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;

        Assert.True(root.GetProperty("isRoot").GetBoolean());

        var tool = root.GetProperty("tools").GetProperty("dotnet-reportgenerator-globaltool");
        Assert.Equal("5.5.12", tool.GetProperty("version").GetString());
        Assert.Contains(
            tool.GetProperty("commands").EnumerateArray().Select(command => command.GetString()),
            command => command == "reportgenerator");
    }

    [Fact]
    public void ThreeJsSubmodule_IsDeclaredAtLibThreeJsAndCheckedOut()
    {
        var gitmodulesPath = Path.Combine(SolutionPathHelper.SolutionRoot, ".gitmodules");
        Assert.True(File.Exists(gitmodulesPath), $"Expected .gitmodules at '{gitmodulesPath}'.");

        var content = File.ReadAllText(gitmodulesPath);
        Assert.Contains("[submodule \"lib/three.js\"]", content, StringComparison.Ordinal);
        Assert.Contains("path = lib/three.js", content, StringComparison.Ordinal);

        var packageJson = Path.Combine(SolutionPathHelper.SolutionRoot, "lib", "three.js", "package.json");
        Assert.True(File.Exists(packageJson), $"Expected the Three.js submodule to be checked out ('{packageJson}').");
    }

    [Theory]
    [InlineData("AGENTS.md")]
    [InlineData("docs/GUIDELINES.md")]
    [InlineData("docs/roadmap/port/phase-01-foundation.md")]
    public void GovernanceDocuments_ArePreserved(string relativePath)
    {
        var path = Path.Combine(SolutionPathHelper.SolutionRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(path), $"Expected governance document at '{path}'.");
        Assert.False(string.IsNullOrWhiteSpace(File.ReadAllText(path)), $"Governance document '{relativePath}' is empty.");
    }
}

