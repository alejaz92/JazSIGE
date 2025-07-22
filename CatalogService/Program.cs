using CatalogService.Business.Interfaces;
using CatalogService.Business.Services;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Database Configuration
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDB")));

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

// test3

// JWT Configuration
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


// declare repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IGrossIncomeTypeRepository, GrossIncomeTypeRepository>();
builder.Services.AddScoped<IIVATypeRepository, IVATypeRepository>();
builder.Services.AddScoped<ILineGroupRepository, LineGroupRepository>();
builder.Services.AddScoped<ILineRepository, LineRepository>();
builder.Services.AddScoped<IPostalCodeRepository, PostalCodeRepository>();
builder.Services.AddScoped<IPriceListRepository, PriceListRepository>();
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<ISellConditionRepository, SellConditionRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ITransportRepository, TransportRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

// declare services
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IArticleValidatorService, ArticleValidatorService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerValidatorService, CustomerValidatorService>();
builder.Services.AddScoped<IGrossIncomeTypeService, GrossIncomeTypeService>();
builder.Services.AddScoped<IIVATypeService, IVATypeService>();
builder.Services.AddScoped<ILineGroupService, LineGroupService>();
builder.Services.AddScoped<ILineService, LineService>();
builder.Services.AddScoped<ILineValidatorService, LineValidatorService>();
builder.Services.AddScoped<IPostalCodeService, PostalCodeService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<ISellConditionService, SellConditionService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISupplierValidatorService, SupplierValidatorService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStockServiceClient, StockServiceClient>();


//inyect configuration
builder.Services.AddHttpClient();
builder.Services.Configure<AuthServiceSettings>(builder.Configuration.GetSection("AuthService"));
builder.Services.AddHttpContextAccessor();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(" CatalogService iniciado correctamente en entorno: {env}", app.Environment.EnvironmentName);



app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CatalogService v1");
    c.RoutePrefix = string.Empty;  // Podés cambiar el prefijo o dejarlo vacío
});


app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
