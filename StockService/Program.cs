using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockService.Business.Interfaces;
using StockService.Business.Services;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Repositories;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Agregar contexto y repositorios
builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Swagger Configuration
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


// Contexto HTTP para extraer token
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Repositories
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IStockByDispatchRepository, StockByDispatchRepository>();
builder.Services.AddScoped<IPendingStockEntryRepository, PendingStockEntryRepository>();
builder.Services.AddScoped<ICommitedStockEntryRepository, CommitedStockEntryRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();

// Services
builder.Services.AddScoped<IStockService, StockService.Business.Services.StockService>();
builder.Services.AddScoped<ICatalogValidatorService, CatalogValidatorService>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();
builder.Services.AddScoped<IEnumService, EnumService>();
builder.Services.AddScoped<IPendingStockService, PendingStockService>();
builder.Services.AddScoped<ICommitedStockService, CommitedStockService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();



// Autenticación JWT (si este microservicio va a estar expuesto)
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockService API v1");
        c.RoutePrefix = "swagger";  // Podés cambiar el prefijo o dejarlo vacío
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
