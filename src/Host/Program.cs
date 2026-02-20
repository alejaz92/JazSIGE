using Catalog.Api;
using Sales.Api;
using Accounting.Api;
using SharedKernel.Modularity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddModuleInstallers(
    builder.Configuration,
    typeof(CatalogModuleInstaller),
    typeof(SalesModuleInstaller),
    typeof(AccountingModuleInstaller));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok("JazSIGE modular host running"));
app.MapModuleEndpoints();

app.Run();
