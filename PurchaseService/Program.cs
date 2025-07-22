using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PurchaseService.Infrastructure.Data;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Repositories;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Services;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);
// test33

// DB
builder.Services.AddDbContext<PurchaseDbContext>(options =>
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
    .AddJwtBearer(options => {
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
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IDispatchRepository, DispatchRepository>();


//Services
builder.Services.AddScoped<IDispatchService, DispatchService>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();
builder.Services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();
builder.Services.AddScoped<IStockServiceClient, StockServiceClient>();
builder.Services.AddScoped<IPurchaseService, PurchaseService.Business.Services.PurchaseService>();

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PurchaseService v1");
    c.RoutePrefix = string.Empty;  // Podés cambiar el prefijo o dejarlo vacío
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();