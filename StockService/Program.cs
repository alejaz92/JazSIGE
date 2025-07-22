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

// test3

// DB
builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// test2 deploy
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

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Repositories
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IStockByDispatchRepository, StockByDispatchRepository>();
builder.Services.AddScoped<IPendingStockEntryRepository, PendingStockEntryRepository>();
builder.Services.AddScoped<ICommitedStockEntryRepository, CommitedStockEntryRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();

// Services
builder.Services.AddScoped<IStockService, StockService.Business.Services.StockService>();
builder.Services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();
builder.Services.AddScoped<ICompanyServiceClient, CompanyServiceClient>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();
builder.Services.AddScoped<IEnumService, EnumService>();
builder.Services.AddScoped<IPendingStockService, PendingStockService>();
builder.Services.AddScoped<ICommitedStockService, CommitedStockService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();


//inyect configuration
builder.Services.AddHttpClient();
//builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));
builder.Services.AddHttpContextAccessor();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockService v1");
    c.RoutePrefix = string.Empty;  // Podés cambiar el prefijo o dejarlo vacío
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



