using System.Reflection;
using System.Runtime.Versioning;

namespace Three.Tests;

public class AssemblyLoadingTests
{
    [Theory]
    [InlineData("Three.cs")]
    [InlineData("Three.Engine")]
    public void Assembly_LoadsByNameSuccessfully(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);

        Assert.NotNull(assembly);
        Assert.Equal(assemblyName, assembly.GetName().Name);
    }

    [Theory]
    [InlineData("Three.cs")]
    [InlineData("Three.Engine")]
    public void Assembly_TargetsExpectedFramework(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var targetFramework = assembly.GetCustomAttribute<TargetFrameworkAttribute>();

        Assert.NotNull(targetFramework);
        Assert.StartsWith(".NETCoreApp,Version=v10.0", targetFramework.FrameworkName, StringComparison.Ordinal);
    }
}

