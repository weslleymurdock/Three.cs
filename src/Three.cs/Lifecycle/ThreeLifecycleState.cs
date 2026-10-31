namespace Three.Lifecycle;

/// <summary>
/// Represents the discrete lifecycle states of Three.cs components and runtime.
/// </summary>
public enum ThreeLifecycleState
{
    /// <summary>
    /// The component is instantiated but has not started initialization.
    /// </summary>
    Uninitialized,

    /// <summary>
    /// The component is currently performing asynchronous initialization.
    /// </summary>
    Initializing,

    /// <summary>
    /// The component is successfully initialized and ready for execution.
    /// </summary>
    Initialized,

    /// <summary>
    /// The component is currently executing disposal and releasing resources.
    /// </summary>
    Disposing,

    /// <summary>
    /// The component has completed disposal and cannot be reused.
    /// </summary>
    Disposed
}
