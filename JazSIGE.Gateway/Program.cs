using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

//  CONFIGURACIÓN JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});




var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");


app.UseAuthentication(); //  Autenticación primero vi
app.UseAuthorization();  //  Luego autorización

app.Use(async (context, next) =>
{
    var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (userIdClaim != null)
    {
        context.Request.Headers["X-UserId"] = userIdClaim.Value;
    }

    await next();
});




app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseOcelot().Wait();

app.Run();
