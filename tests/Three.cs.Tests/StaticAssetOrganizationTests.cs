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
    public void StaticAssets_DirectoryExists(string directoryName)
    {
        var directoryPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "wwwroot", directoryName);
        Assert.True(Directory.Exists(directoryPath), $"Expected static asset directory '{directoryName}' at '{directoryPath}'.");
    }

    [Fact]
    public void StaticAssets_ThreeDirectory_ContainsRequiredAssets()
    {
        var threeDir = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "wwwroot", "three");
        Assert.True(File.Exists(Path.Combine(threeDir, "three.module.js")), "three.module.js missing");
        Assert.True(File.Exists(Path.Combine(threeDir, "three.core.js")), "three.core.js missing");
        Assert.True(File.Exists(Path.Combine(threeDir, "LICENSE")), "LICENSE missing");
        Assert.True(File.Exists(Path.Combine(threeDir, "upstream.json")), "upstream.json missing");
    }

    [Fact]
    public void StaticAssets_JsDirectory_ContainsEntryPoints()
    {
        var jsDir = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "wwwroot", "js");
        Assert.True(File.Exists(Path.Combine(jsDir, "three-runtime.js")), "three-runtime.js missing");
        Assert.True(File.Exists(Path.Combine(jsDir, "three-runtime-core.js")), "three-runtime-core.js missing");
        Assert.True(File.Exists(Path.Combine(jsDir, "three-upstream.js")), "three-upstream.js missing");
    }
}
