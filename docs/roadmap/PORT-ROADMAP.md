# Three.cs Port Roadmap

This roadmap governs the port of Three.js into `src/Three.cs` and its consumption from Blazor WebAssembly and .NET MAUI Hybrid hosts.

## Phases

1. Foundation — normalize the existing projects without replacing them.
2. Three.js runtime — integrate a pinned upstream Three.js revision and selected modules.
3. C# bindings and MAUI bridge — expose typed APIs and WebView lifecycle integration.
4. Cross-host validation — validate browser and MAUI targets and package the library.

## Architecture

```text
lib/three.js -> Three.cs -> Blazor WASM / MAUI Hybrid WebView
                         \\-> Three.Engine
```

The existing `lib/three.js` git submodule, `src/Three.cs/Three.cs.csproj`, `src/Three.Engine/Three.Engine.csproj`, and `Three.cs.slnx` are project foundations and must not be recreated or replaced by agents.

## Definition of done

A supported C# abstraction can initialize, use, and dispose the supported Three.js runtime through the same public API on Blazor WASM and MAUI Hybrid Android, iOS, MacCatalyst, and Windows hosts.

Each phase has a dedicated checklist under `docs/roadmap/port/`. Agents must only mark checklist items complete after implementation and validation evidence exists.