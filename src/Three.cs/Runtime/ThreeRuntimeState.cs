namespace Three.Runtime;

/// <summary>Lifecycle state of an <see cref="IThreeRuntime"/>.</summary>
public enum ThreeRuntimeState
{
    /// <summary>The runtime has not been initialized yet (or an initialization attempt was cancelled).</summary>
    Uninitialized = 0,

    /// <summary>An initialization attempt is in flight.</summary>
    Initializing = 1,

    /// <summary>The Three.js runtime is loaded and verified.</summary>
    Ready = 2,

    /// <summary>The last initialization attempt failed. A new attempt may be made.</summary>
    Failed = 3,

    /// <summary>The runtime was disposed. It cannot be used again.</summary>
    Disposed = 4,
}