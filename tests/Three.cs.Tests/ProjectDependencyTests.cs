using System.Xml.Linq;

namespace Three.Tests;

public class ProjectDependencyTests
{
    [Fact]
    public void ThreeEngine_DeclaresProjectReferenceTo_ThreeCs()
    {
        var engineCsprojPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.Engine", "Three.Engine.csproj");
        Assert.True(File.Exists(engineCsprojPath), $"Could not find Three.Engine.csproj at '{engineCsprojPath}'.");

        var doc = XDocument.Load(engineCsprojPath);
        var projectReferences = doc.Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrEmpty(value))
            .ToList();

        Assert.NotEmpty(projectReferences);

        var matchesThreeCs = projectReferences.Any(reference =>
        {
            var normalized = reference!.Replace('\\', '/');
            return normalized.EndsWith("Three.cs/Three.cs.csproj", StringComparison.OrdinalIgnoreCase);
        });

        Assert.True(matchesThreeCs, "Three.Engine.csproj must declare a ProjectReference to Three.cs.csproj.");

        var engineDirectory = Path.GetDirectoryName(engineCsprojPath)!;
        foreach (var reference in projectReferences)
        {
            var resolvedTarget = Path.GetFullPath(Path.Combine(engineDirectory, reference!));
            Assert.True(File.Exists(resolvedTarget), $"Referenced project file not found at '{resolvedTarget}'.");
        }
    }

    [Theory]
    [InlineData("src/Three.cs/Three.cs.csproj")]
    [InlineData("src/Three.Engine/Three.Engine.csproj")]
    [InlineData("tests/Three.cs.Tests/Three.cs.Tests.csproj")]
    public void SolutionStructure_ContainsRequiredProjectAndFileExists(string expectedProjectPath)
    {
        var slnxPath = Path.Combine(SolutionPathHelper.SolutionRoot, "Three.cs.slnx");
        Assert.True(File.Exists(slnxPath), $"Could not find Three.cs.slnx at '{slnxPath}'.");

        var doc = XDocument.Load(slnxPath);
        var registeredProjects = doc.Descendants("Project")
            .Select(element => element.Attribute("Path")?.Value?.Replace('\\', '/'))
            .Where(value => !string.IsNullOrEmpty(value))
            .ToList();

        Assert.Contains(expectedProjectPath, registeredProjects);

        var resolvedPath = Path.Combine(SolutionPathHelper.SolutionRoot, expectedProjectPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(resolvedPath), $"Project declared in solution does not exist at '{resolvedPath}'.");
    }
}

