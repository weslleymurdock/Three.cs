using System.Xml.Linq;

namespace Three.Tests;

public class ProjectConfigurationTests
{
    [Fact]
    public void ThreeCs_ConfiguredWithRazorSdkAndWebComponents()
    {
        var csprojPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "Three.cs.csproj");
        Assert.True(File.Exists(csprojPath), $"Could not find Three.cs.csproj at '{csprojPath}'.");

        var doc = XDocument.Load(csprojPath);
        var sdkAttribute = doc.Root?.Attribute("Sdk")?.Value;
        Assert.Equal("Microsoft.NET.Sdk.Razor", sdkAttribute);

        var supportedPlatform = doc.Descendants("SupportedPlatform")
            .Select(element => element.Attribute("Include")?.Value)
            .FirstOrDefault();
        Assert.Equal("browser", supportedPlatform);

        var webPackage = doc.Descendants("PackageReference")
            .FirstOrDefault(element => element.Attribute("Include")?.Value == "Microsoft.AspNetCore.Components.Web");
        Assert.NotNull(webPackage);
    }

    [Theory]
    [InlineData("src/Three.cs/Three.cs.csproj")]
    [InlineData("src/Three.Engine/Three.Engine.csproj")]
    [InlineData("tests/Three.cs.Tests/Three.cs.Tests.csproj")]
    public void Projects_HaveNullableAndImplicitUsingsEnabled(string relativeProjectPath)
    {
        var projectPath = Path.Combine(SolutionPathHelper.SolutionRoot, relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(projectPath), $"Project file not found at '{projectPath}'.");

        var doc = XDocument.Load(projectPath);
        var nullable = doc.Descendants("Nullable").FirstOrDefault()?.Value?.Trim();
        var implicitUsings = doc.Descendants("ImplicitUsings").FirstOrDefault()?.Value?.Trim();

        Assert.Equal("enable", nullable);
        Assert.Equal("enable", implicitUsings);
    }

    [Theory]
    [InlineData("src/Three.cs/Three.cs.csproj")]
    [InlineData("src/Three.Engine/Three.Engine.csproj")]
    [InlineData("tests/Three.cs.Tests/Three.cs.Tests.csproj")]
    public void Projects_TargetDotNet10(string relativeProjectPath)
    {
        var projectPath = Path.Combine(SolutionPathHelper.SolutionRoot, relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(projectPath), $"Project file not found at '{projectPath}'.");

        var doc = XDocument.Load(projectPath);
        var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value?.Trim();

        Assert.Equal("net10.0", targetFramework);
    }

    [Fact]
    public void ThreeCs_ConfiguredWithRootNamespaceThree()
    {
        var csprojPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "Three.cs.csproj");
        Assert.True(File.Exists(csprojPath), $"Could not find Three.cs.csproj at '{csprojPath}'.");

        var doc = XDocument.Load(csprojPath);
        var rootNamespace = doc.Descendants("RootNamespace").FirstOrDefault()?.Value?.Trim();

        Assert.Equal("Three", rootNamespace);
    }
}
