using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SharedKernel.Modularity;

public static class ModuleServiceCollectionExtensions
{
    public static IServiceCollection AddModuleInstallers(
        this IServiceCollection services,
        IConfiguration configuration,
        params Type[] installerMarkers)
    {
        var installers = installerMarkers
            .Select(marker => marker.Assembly)
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IModuleInstaller).IsAssignableFrom(type))
            .Select(type => (IModuleInstaller)Activator.CreateInstance(type)!)
            .ToList();

        foreach (var installer in installers)
        {
            installer.ConfigureServices(services, configuration);
        }

        services.TryAddSingleton<IReadOnlyCollection<IModuleInstaller>>(installers);
        return services;
    }

    public static WebApplication MapModuleEndpoints(this WebApplication app)
    {
        var installers = app.Services.GetRequiredService<IReadOnlyCollection<IModuleInstaller>>();

        foreach (var installer in installers)
        {
            installer.MapEndpoints(app);
        }

        return app;
    }

    public static string BuildVersionedPrefix(this IModuleInstaller installer) =>
        $"{installer.RoutePrefix.TrimEnd('/')}/v{installer.ApiVersion}";
}
