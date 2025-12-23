using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Middlewares;
using FiscalDocumentationService.Business.Options;
using FiscalDocumentationService.Business.Services;
using FiscalDocumentationService.Business.Services.Clients;
using FiscalDocumentationService.Infrastructure.Data;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<FiscalDocumentationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("https://gateway-api-dev-hjasdzc6dggka6ah.brazilsouth-01.azurewebsites.net", "https://localhost:4200", "https://localhost:7273") // Frontend local
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories y UoW
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();

// Services
builder.Services.AddScoped<IFiscalDocumentService, FiscalDocumentService>();
//builder.Services.AddScoped<IArcaServiceClient, ArcaServiceClient>();
//builder.Services.AddScoped<ICompanyServiceClient, CompanyServiceClient>();
//builder.Services.AddScoped<IArcaAuthClient, ArcaAuthClient>();
builder.Services.AddSingleton<IArcaAccessTicketCache, ArcaAccessTicketCache>();

builder.Services.Configure<ArcaOptions>(builder.Configuration.GetSection("Arca"));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ArcaOptions>>().Value);

//inyect configuration
builder.Services.AddHttpClient<ICompanyServiceClient, CompanyServiceClient>();
builder.Services.AddHttpClient<IArcaServiceClient, ArcaServiceClient>();
builder.Services.AddHttpClient<IArcaAuthClient, ArcaAuthClient>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FiscalDocumentationService v1");
    c.RoutePrefix = string.Empty;  // Podés cambiar el prefijo o dejarlo vacío
});

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
