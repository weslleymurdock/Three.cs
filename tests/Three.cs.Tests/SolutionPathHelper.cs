namespace Three.Tests;

internal static class SolutionPathHelper
{
    private static readonly Lazy<string> SolutionRootLazy = new(FindSolutionRoot);

    public static string SolutionRoot => SolutionRootLazy.Value;

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
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate solution root containing Three.cs.slnx");
    }
}
