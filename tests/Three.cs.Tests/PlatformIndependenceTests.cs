using System.Reflection;
using System.Xml.Linq;

namespace Three.Tests;

public class PlatformIndependenceTests
{
    private static readonly string[] PlatformPrefixes =
    [
        "Microsoft.Maui",
        "Microsoft.iOS",
        "Microsoft.Android",
        "Microsoft.MacCatalyst",
        "Microsoft.WindowsDesktop",
        "WindowsBase"
    ];

    [Theory]
    [InlineData("Three.cs")]
    [InlineData("Three.Engine")]
    public void Assemblies_DoNotReferencePlatformAssemblies(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        foreach (var referenced in referencedAssemblies)
        {
            foreach (var platform in PlatformPrefixes)
            {
                Assert.False(
                    referenced.Name?.StartsWith(platform, StringComparison.OrdinalIgnoreCase) ?? false,
                    $"Assembly '{assemblyName}' references platform-specific assembly '{referenced.Name}'."
                );
            }
        }
    }

    [Theory]
    [InlineData("src/Three.cs/Three.cs.csproj")]
    [InlineData("src/Three.Engine/Three.Engine.csproj")]
    public void Projects_DoNotTargetPlatformSpecificFrameworks(string relativeProjectPath)
    {
        var projectPath = Path.Combine(SolutionPathHelper.SolutionRoot, relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(projectPath), $"Project file not found at '{projectPath}'.");

        var doc = XDocument.Load(projectPath);
        var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value?.Trim();

        Assert.NotNull(targetFramework);
        Assert.Equal("net10.0", targetFramework);
        Assert.DoesNotContain("-", targetFramework);
    }

    [Theory]
    [InlineData("src/Three.cs/Three.cs.csproj")]
    [InlineData("src/Three.Engine/Three.Engine.csproj")]
    public void Projects_DoNotReferencePlatformPackages(string relativeProjectPath)
    {
        var projectPath = Path.Combine(SolutionPathHelper.SolutionRoot, relativeProjectPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(projectPath), $"Project file not found at '{projectPath}'.");

        var doc = XDocument.Load(projectPath);
        var packageReferences = doc.Descendants("PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrEmpty(value))
            .ToList();

        foreach (var package in packageReferences)
        {
            foreach (var platform in PlatformPrefixes)
            {
                Assert.False(
                    package!.StartsWith(platform, StringComparison.OrdinalIgnoreCase),
                    $"Project '{relativeProjectPath}' references platform-specific package '{package}'."
                );
            }
        }
    }
}
