using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Infrastructure;
using SharedKernel.Modularity;

namespace Sales.Api;

public sealed class SalesModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Sales";
    public string RoutePrefix => "/api/sales";
    public string ApiVersion => "1";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISalesRepository, InMemorySalesRepository>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this.BuildVersionedPrefix())
            .WithTags(ModuleName);

        group.MapGet("/orders", (ISalesRepository repository) => Results.Ok(repository.GetAll()));
    }
}
