using System.Reflection;
using System.Text.Json;

namespace Three.Tests;

public class FoundationSmokeTests
{
    private static readonly string SolutionRoot = FindSolutionRoot();

    [Fact]
    public void ThreeAssembly_LoadsSuccessfully()
    {
        var assembly = Assembly.Load("Three.cs");
        Assert.NotNull(assembly);
    }

    [Fact]
    public void ThreeEngineAssembly_LoadsSuccessfully()
    {
        var assembly = Assembly.Load("Three.Engine");
        Assert.NotNull(assembly);
    }

    [Fact]
    public void ThreeEngine_HasProjectReferenceTo_ThreeCs()
    {
        var csprojPath = Path.Combine(SolutionRoot, "src", "Three.Engine", "Three.Engine.csproj");
        Assert.True(File.Exists(csprojPath), $"Could not find Three.Engine.csproj at '{csprojPath}'.");

        var csprojContent = File.ReadAllText(csprojPath);
        Assert.Contains(@"..\Three.cs\Three.cs.csproj", csprojContent);

        // Verify resolved project dependency in project.assets.json
        var assetsPath = Path.Combine(SolutionRoot, "src", "Three.Engine", "obj", "project.assets.json");
        if (File.Exists(assetsPath))
        {
            var assetsJson = File.ReadAllText(assetsPath);
            Assert.Contains("Three.cs", assetsJson);
        }
    }

    [Theory]
    [InlineData("Three.cs")]
    [InlineData("Three.Engine")]
    public void Assemblies_ArePlatformIndependent(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var referencedAssemblies = assembly.GetReferencedAssemblies();

        var platformPrefixes = new[]
        {
            "Microsoft.Maui",
            "Microsoft.iOS",
            "Microsoft.Android",
            "Microsoft.MacCatalyst",
            "Microsoft.WindowsDesktop",
            "WindowsBase"
        };

        foreach (var referenced in referencedAssemblies)
        {
            foreach (var platform in platformPrefixes)
            {
                Assert.False(
                    referenced.Name?.StartsWith(platform, StringComparison.OrdinalIgnoreCase) ?? false,
                    $"Assembly '{assemblyName}' references platform-specific assembly '{referenced.Name}'."
                );
            }
        }
    }

    [Fact]
    public void ThreeCs_IsConfiguredAsRazorClassLibrary()
    {
        var csprojPath = Path.Combine(SolutionRoot, "src", "Three.cs", "Three.cs.csproj");
        Assert.True(File.Exists(csprojPath), $"Could not find Three.cs.csproj at '{csprojPath}'.");

        var content = File.ReadAllText(csprojPath);
        Assert.Contains(@"Sdk=""Microsoft.NET.Sdk.Razor""", content);
        Assert.Contains("<TargetFramework>net10.0</TargetFramework>", content);
        Assert.Contains("<Nullable>enable</Nullable>", content);
        Assert.Contains("Microsoft.AspNetCore.Components.Web", content);
    }

    [Theory]
    [InlineData("Bindings")]
    [InlineData("Interop")]
    [InlineData("Runtime")]
    [InlineData("Lifecycle")]
    [InlineData("Hosting")]
    public void ThreeCs_ContainsExpectedArchitectureDirectories(string directoryName)
    {
        var path = Path.Combine(SolutionRoot, "src", "Three.cs", directoryName);
        Assert.True(Directory.Exists(path), $"Expected directory '{directoryName}' at '{path}'.");
    }

    [Theory]
    [InlineData("three")]
    [InlineData("js")]
    public void ThreeCs_StaticAssetsStructure_ContainsExpectedDirectories(string directoryName)
    {
        var path = Path.Combine(SolutionRoot, "src", "Three.cs", "wwwroot", directoryName);
        Assert.True(Directory.Exists(path), $"Expected static asset directory '{directoryName}' at '{path}'.");
    }

    [Fact]
    public void SolutionStructure_ContainsAllRequiredProjects()
    {
        var slnxPath = Path.Combine(SolutionRoot, "Three.cs.slnx");
        Assert.True(File.Exists(slnxPath), $"Could not find Three.cs.slnx at '{slnxPath}'.");

        var content = File.ReadAllText(slnxPath);
        Assert.Contains(@"src/Three.cs/Three.cs.csproj", content);
        Assert.Contains(@"src/Three.Engine/Three.Engine.csproj", content);
    }

    private static string FindSolutionRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "Three.cs.slnx")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent == null) break;
            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate solution root with Three.cs.slnx");
    }
}
