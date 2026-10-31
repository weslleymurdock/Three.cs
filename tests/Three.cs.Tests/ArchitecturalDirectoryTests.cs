namespace Three.Tests;

public class ArchitecturalDirectoryTests
{
    [Theory]
    [InlineData("Bindings")]
    [InlineData("Hosting")]
    [InlineData("Interop")]
    [InlineData("Lifecycle")]
    [InlineData("Runtime")]
    public void ArchitectureDirectory_AndGitMarkerExist(string directoryName)
    {
        var directoryPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", directoryName);
        Assert.True(Directory.Exists(directoryPath), $"Expected architecture directory '{directoryName}' at '{directoryPath}'.");

        var gitkeepPath = Path.Combine(directoryPath, ".gitkeep");
        Assert.True(File.Exists(gitkeepPath), $"Expected .gitkeep tracking marker at '{gitkeepPath}'.");
    }
}

