using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Modularity;

public interface IModuleInstaller
{
    string ModuleName { get; }
    string RoutePrefix { get; }
    string ApiVersion { get; }

    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
