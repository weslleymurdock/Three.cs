namespace Three.Tests;

public class StaticAssetOrganizationTests
{
    [Fact]
    public void StaticAssets_WwwrootDirectory_Exists()
    {
        var wwwrootPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "wwwroot");
        Assert.True(Directory.Exists(wwwrootPath), $"Expected wwwroot directory at '{wwwrootPath}'.");
    }

    [Theory]
    [InlineData("three")]
    [InlineData("js")]
    public void StaticAssets_DirectoryAndGitMarkerExist(string directoryName)
    {
        var directoryPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "wwwroot", directoryName);
        Assert.True(Directory.Exists(directoryPath), $"Expected static asset directory '{directoryName}' at '{directoryPath}'.");

        var gitkeepPath = Path.Combine(directoryPath, ".gitkeep");
        Assert.True(File.Exists(gitkeepPath), $"Expected .gitkeep tracking marker at '{gitkeepPath}'.");
    }
}

