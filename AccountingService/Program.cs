using AccountingService.Business.Interfaces;
using AccountingService.Business.Interfaces.Clients;
using AccountingService.Business.Services;
using AccountingService.Business.Services.Clients;
using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Repositories;
using AccountingService.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


//test4
var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AccountingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS Configuration
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

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ILedgerDocumentRepository, LedgerDocumentRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();
builder.Services.AddScoped<INumberingSequenceRepository, NumberingSequenceRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Services
builder.Services.AddScoped<ICustomerAccountQueryService, CustomerAccountQueryService>();
builder.Services.AddScoped<IDocumentIntakeService, DocumentIntakeService>();
builder.Services.AddScoped<IReceiptCommandService, ReceiptCommandService>();
builder.Services.AddScoped<ICompanyServiceClient, CompanyServiceClient>();
builder.Services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();
builder.Services.AddScoped<IReceiptQueryService, ReceiptQueryService>();        




//inyect configuration
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
//builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));
builder.Services.AddHttpContextAccessor();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // serializa enums como texto (camelCase opcional) y ACEPTA enteros al deserializar
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true)
        );
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AccountingService v1");
    c.RoutePrefix = string.Empty;  // Podés cambiar el prefijo o dejarlo vacío
});

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();