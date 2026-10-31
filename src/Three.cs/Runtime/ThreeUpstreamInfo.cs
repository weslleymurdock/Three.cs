namespace Three.Runtime;

/// <summary>Provenance of the Three.js build shipped by the RCL.</summary>
/// <param name="Name">Upstream package name.</param>
/// <param name="Version">Upstream package version (for example <c>0.187.0</c>).</param>
/// <param name="Revision">Upstream <c>REVISION</c> constant the package was built for (for example <c>187</c>).</param>
/// <param name="Commit">Pinned upstream git commit.</param>
/// <param name="License">SPDX license identifier.</param>
/// <param name="Repository">Upstream repository URL.</param>
/// <param name="LoadedRevision">The <c>REVISION</c> reported by the loaded module itself, or <see langword="null"/> before it is loaded.</param>
public sealed record ThreeUpstreamInfo(
    string Name,
    string Version,
    string Revision,
    string Commit,
    string License,
    string Repository,
    string? LoadedRevision);