using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseOcelot().Wait();

app.Run();
