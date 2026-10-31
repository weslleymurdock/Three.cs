namespace Three.Tests;

/// <summary>
/// Guards the Phase 02 boundary defined by Issue #5:
/// - Bindings, Hosting and Lifecycle remain empty (tracking markers only).
/// - Runtime and Interop contain Phase 02 Three.js runtime integration types.
/// - Static assets (wwwroot/three and wwwroot/js) contain the runtime assets and facades.
/// - Three.Engine contains no source files (Phase 01 of Engine is later).
/// </summary>
public class PhaseBoundaryTests
{
    private static readonly string[] SourceExtensions = [".cs", ".razor", ".js", ".mjs", ".ts"];

    [Theory]
    [InlineData("Bindings")]
    [InlineData("Hosting")]
    [InlineData("Lifecycle")]
    public void LaterPhaseArchitectureDirectory_ContainsOnlyTheTrackingMarker(string directoryName)
    {
        var directoryPath = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs", directoryName);

        var entries = Directory.GetFileSystemEntries(directoryPath, "*", SearchOption.AllDirectories)
            .Select(entry => Path.GetRelativePath(directoryPath, entry))
            .ToList();

        Assert.Equal([".gitkeep"], entries);
    }

    [Fact]
    public void ThreeCs_ContainsSourceFilesOnlyInRuntimeAndInterop()
    {
        var projectDirectory = Path.Combine(SolutionPathHelper.SolutionRoot, "src", "Three.cs");

        var sources = EnumerateProjectFiles(projectDirectory)
            .Where(file => SourceExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Select(file => Path.GetRelativePath(projectDirectory, file))
            .Where(file => !file.StartsWith("wwwroot" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.NotEmpty(sources);
        Assert.All(sources, source =>
        {
            var folder = source.Split(Path.DirectorySeparatorChar)[0];
            Assert.True(folder == "Runtime" || folder == "Interop",
                $"Unexpected source file '{source}' outside Runtime/Interop in Phase 02.");
        });
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
