using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using FiscalDocumentationService.Infrastructure.Data;




var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<FiscalDocumentationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("FrontendPolicy", policy => {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Repositories


//Services




builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Contexto HTTP para extraer token
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FiscalDocumentationService API v1");
        c.RoutePrefix = "swagger";  // Podés cambiar el prefijo o dejarlo vacío
    });
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
