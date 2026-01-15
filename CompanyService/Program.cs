using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using CompanyService.Infrastructure.Data;
using CompanyService.Infrastructure.Interfaces;
using CompanyService.Infrastructure.Repositories;
using CompanyService.Infrastructure.Middleware;
using CompanyService.Business.Interfaces;
using CompanyService.Business.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Company Service - Microservice for managing company information
/// Provides company data for invoicing and business process configurations
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Database Configuration
// ============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Database connection string is not configured");

builder.Services.AddDbContext<CompanyDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================================================
// CORS Configuration
// ============================================================================
// Configure CORS policy to allow requests from specified origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("https://gateway-api-dev-hjasdzc6dggka6ah.brazilsouth-01.azurewebsites.net", "https://localhost:7273")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ============================================================================
// JWT Authentication Configuration
// ============================================================================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("JWT Key is not configured");

var key = Encoding.UTF8.GetBytes(jwtKey);

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

// ============================================================================
// Swagger/OpenAPI Configuration
// ============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================================
// Dependency Injection - Repositories
// ============================================================================
builder.Services.AddScoped<ICompanyInfoRepository, CompanyInfoRepository>();

// ============================================================================
// Dependency Injection - Business Services
// ============================================================================
builder.Services.AddScoped<ICompanyInfoService, CompanyInfoService>();
builder.Services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();

// ============================================================================
// HTTP Client Configuration
// ============================================================================
// Configure HTTP client factory for external service calls
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ============================================================================
// Controllers Configuration
// ============================================================================
// Configure API behavior to return consistent validation error responses
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new
            {
                message = "Validation failed",
                errors = errors
            });
        };
    });

var app = builder.Build();

// ============================================================================
// Middleware Pipeline Configuration
// ============================================================================

// Swagger UI (only in development environment)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompanyService v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();

// Global Exception Handler - Must be early in pipeline to catch all errors
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();