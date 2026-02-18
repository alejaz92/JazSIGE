using Catalog.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Modularity;

namespace Catalog.Api;

public sealed class CatalogModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Catalog";
    public string RoutePrefix => "/api/catalog";
    public string ApiVersion => "1";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICatalogRepository, InMemoryCatalogRepository>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this.BuildVersionedPrefix())
            .WithTags(ModuleName);

        group.MapGet("/items", (ICatalogRepository repository) => Results.Ok(repository.GetAll()));
    }
}
