namespace Three.Tests;

/// <summary>
/// Guards the Phase 01 boundary defined by Issue #4: the architecture folders and static asset folders are
/// prepared, but no runtime, bindings, bridge, lifecycle, hosting or bundled Three.js content exists yet.
/// Later phases that intentionally add content must update these expectations as part of their own scope.
/// </summary>
public class PhaseBoundaryTests
{
    private static readonly string[] SourceExtensions = [".cs", ".razor", ".js", ".mjs", ".ts"];

    [Theory]
    [InlineData("Bindings")]
    [InlineData("Hosting")]
    [InlineData("Interop")]
    [InlineData("Lifecycle")]
    [InlineData("Runtime")]
    public void ArchitectureDirectory_ContainsOnlyTheTrackingMarker(string directoryName)
    {
        var directoryPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", directoryName);

        var entries = Directory.GetFileSystemEntries(directoryPath, "*", SearchOption.AllDirectories)
            .Select(entry => Path.GetRelativePath(directoryPath, entry))
            .ToList();

        Assert.Equal([".gitkeep"], entries);
    }

    [Theory]
    [InlineData("three")]
    [InlineData("js")]
    public void StaticAssetDirectory_ContainsNoBundledAssetsYet(string directoryName)
    {
        var directoryPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", "wwwroot", directoryName);

        var entries = Directory.GetFileSystemEntries(directoryPath, "*", SearchOption.AllDirectories)
            .Select(entry => Path.GetRelativePath(directoryPath, entry))
            .ToList();

        Assert.Equal([".gitkeep"], entries);
    }

    [Fact]
    public void ThreeCs_ContainsNoSourceFilesOutsideStaticAssets()
    {
        var projectDirectory = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs");

        var sources = EnumerateProjectFiles(projectDirectory)
            .Where(file => SourceExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(projectDirectory, file))
            .ToList();

        Assert.Empty(sources);
    }

    [Fact]
    public void ThreeEngine_ContainsNoSourceFiles()
    {
        var projectDirectory = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.Engine");

        var sources = EnumerateProjectFiles(projectDirectory)
            .Where(file => SourceExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(projectDirectory, file))
            .ToList();

        Assert.Empty(sources);
    }

    private static IEnumerable<string> EnumerateProjectFiles(string projectDirectory) =>
        Directory.EnumerateFiles(projectDirectory, "*", SearchOption.AllDirectories)
            .Where(file =>
            {
                var relative = Path.GetRelativePath(projectDirectory, file);
                var firstSegment = relative.Split(Path.DirectorySeparatorChar)[0];
                return !string.Equals(firstSegment, "bin", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(firstSegment, "obj", StringComparison.OrdinalIgnoreCase);
            });
}

