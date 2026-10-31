using Microsoft.Extensions.DependencyInjection;
using Three.Interop;
using Three.Runtime;

namespace Three.Hosting;

/// <summary>
/// Extension methods for configuring Three.cs services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core Three.cs hosting services, JS bridge, and runtime with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">An optional delegate to configure <see cref="ThreeOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddThree(this IServiceCollection services, Action<ThreeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddScoped<IJsBridge, JsBridge>();
        services.AddScoped<IThreeRuntime, ThreeRuntime>();

        return services;
    }
}
