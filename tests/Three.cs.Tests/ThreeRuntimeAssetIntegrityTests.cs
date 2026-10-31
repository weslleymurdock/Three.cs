using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Three.Interop;

namespace Three.Tests;

public class ThreeRuntimeAssetIntegrityTests
{
    [Fact]
    public void AssetConstants_MatchExpectedStaticAssetPaths()
    {
        Assert.Equal("_content/Three.cs/js/three-runtime.js", ThreeRuntimeAssets.ModulePath);
        Assert.Equal("./_content/Three.cs/js/three-runtime.js", ThreeRuntimeAssets.ModuleSpecifier);
    }

    [Fact]
    public void UpstreamMetadata_MatchesShippedAssetsAndSubmodule()
    {
        var solutionRoot = SolutionPathHelper.SolutionRoot;
        var upstreamJsonPath = Path.Combine(solutionRoot, "src", "Three.cs", "wwwroot", "three", "upstream.json");
        Assert.True(File.Exists(upstreamJsonPath), "upstream.json must exist in wwwroot/three");

        using var jsonDoc = JsonDocument.Parse(File.ReadAllText(upstreamJsonPath));
        var root = jsonDoc.RootElement;

        Assert.Equal("three", root.GetProperty("name").GetString());
        Assert.Equal("0.187.0", root.GetProperty("version").GetString());
        Assert.Equal("187", root.GetProperty("revision").GetString());
        Assert.Equal("cb0a902b668f848b2a0b6e1fb8f928f5a8ef1168", root.GetProperty("commit").GetString());
        Assert.Equal("MIT", root.GetProperty("license").GetString());
        Assert.Equal("https://github.com/mrdoob/three.js", root.GetProperty("repository").GetString());

        var licenseSha256 = root.GetProperty("licenseSha256").GetString();
        var licensePath = Path.Combine(solutionRoot, "src", "Three.cs", "wwwroot", "three", "LICENSE");
        Assert.True(File.Exists(licensePath));
        var computedLicenseSha = ComputeSha256(licensePath);
        Assert.Equal(licenseSha256, computedLicenseSha);

        // Verify JavaScript provenance metadata module matches upstream.json
        var jsPath = Path.Combine(solutionRoot, "src", "Three.cs", "wwwroot", "js", "three-upstream.js");
        Assert.True(File.Exists(jsPath), "three-upstream.js must exist in wwwroot/js");
        var jsContent = File.ReadAllText(jsPath);
        Assert.Contains($"name: '{root.GetProperty("name").GetString()}'", jsContent);
        Assert.Contains($"version: '{root.GetProperty("version").GetString()}'", jsContent);
        Assert.Contains($"revision: '{root.GetProperty("revision").GetString()}'", jsContent);
        Assert.Contains($"commit: '{root.GetProperty("commit").GetString()}'", jsContent);
        Assert.Contains($"license: '{root.GetProperty("license").GetString()}'", jsContent);
        Assert.Contains($"repository: '{root.GetProperty("repository").GetString()}'", jsContent);

        foreach (var fileElem in root.GetProperty("files").EnumerateArray())
        {
            var relativePath = fileElem.GetProperty("path").GetString()!;
            var expectedSha = fileElem.GetProperty("sha256").GetString()!;
            var expectedBytes = fileElem.GetProperty("bytes").GetInt64();

            var rclFilePath = Path.Combine(solutionRoot, "src", "Three.cs", "wwwroot", "three", relativePath);
            Assert.True(File.Exists(rclFilePath), $"Asset file missing: {rclFilePath}");

            var fileInfo = new FileInfo(rclFilePath);
            Assert.Equal(expectedBytes, fileInfo.Length);
            Assert.Equal(expectedSha, ComputeSha256(rclFilePath));

            // Verify byte-for-byte match against submodule
            var sourceRel = fileElem.GetProperty("source").GetString()!;
            var submoduleFilePath = Path.Combine(solutionRoot, "lib", "three.js", sourceRel.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(submoduleFilePath), $"Submodule source file missing: {submoduleFilePath}");

            var subInfo = new FileInfo(submoduleFilePath);
            Assert.Equal(subInfo.Length, fileInfo.Length);
            Assert.Equal(ComputeSha256(submoduleFilePath), expectedSha);
        }
    }

    [Fact]
    public void ThreeModule_OnlyImportsThreeCore()
    {
        var solutionRoot = SolutionPathHelper.SolutionRoot;
        var modulePath = Path.Combine(solutionRoot, "src", "Three.cs", "wwwroot", "three", "three.module.js");
        var lines = File.ReadAllLines(modulePath);

        var importLines = lines.Where(l => l.TrimStart().StartsWith("import ")).ToList();
        Assert.NotEmpty(importLines);
        Assert.All(importLines, line =>
        {
            Assert.Contains("from './three.core.js'", line);
        });
    }

    [Fact]
    public void SyncScript_CheckMode_PassesWithoutDrift()
    {
        var solutionRoot = SolutionPathHelper.SolutionRoot;
        var scriptPath = Path.Combine(solutionRoot, "scripts", "Sync-ThreeRuntime.ps1");

        var psi = new ProcessStartInfo("pwsh", $"-NoProfile -File \"{scriptPath}\" -Check")
        {
            WorkingDirectory = solutionRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit(15000);
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        Assert.True(process.ExitCode == 0, $"Sync-ThreeRuntime.ps1 -Check failed (exit {process.ExitCode}): {stdout}\n{stderr}");
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hash);
    }
}

