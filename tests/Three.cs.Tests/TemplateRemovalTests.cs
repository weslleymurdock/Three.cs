namespace Three.Tests;

public class TemplateRemovalTests
{
    [Theory]
    [InlineData("Program.cs")]
    [InlineData("appsettings.json")]
    [InlineData("appsettings.Development.json")]
    [InlineData("Pages")]
    [InlineData("Properties")]
    [InlineData("wwwroot/favicon.ico")]
    [InlineData("wwwroot/css")]
    [InlineData("wwwroot/lib")]
    public void ThreeCs_DoesNotRetainAspNetCoreTemplateHost(string relativePath)
    {
        var path = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.False(File.Exists(path) || Directory.Exists(path), $"Obsolete web host template artifact still exists at '{path}'.");
    }

    [Fact]
    public void ThreeEngine_DoesNotRetainClassLibraryPlaceholder()
    {
        var path = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.Engine", "Class1.cs");

        Assert.False(File.Exists(path), $"Template placeholder still exists at '{path}'.");
    }
}
