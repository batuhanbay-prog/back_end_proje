using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;
using System.Text;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Ortam değişkenlerinden konfigürasyon (12 Faktör - Konfigürasyon) ---
builder.Configuration.AddEnvironmentVariables();

// --- DbContext (SQL Server) ---
var authConnStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(authConnStr));

// --- Health Checks (10.12 - Admin Prosesleri) ---
builder.Services.AddHealthChecks()
    .AddSqlServer(authConnStr, name: "sqlserver", tags: new[] { "db" });

// --- Microsoft Identity ---
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// --- JWT Authentication ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// --- Role-Based ve Policy-Based Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// --- Dependency Injection (DI) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger + JWT desteği ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Auth Mikroservis - JWT token üretme ve kimlik doğrulama"
    });

    // JWT token desteği (Swagger UI'da "Authorize" butonu)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token'ınızı buraya yazın: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // XML yorumlarını Swagger'a aktar
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// --- Global Exception Middleware ---
app.UseMiddleware<GlobalExceptionMiddleware>();

// --- Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Authentication & Authorization ---
app.UseAuthentication();
app.UseAuthorization();

// --- Map Controllers ---
app.MapControllers();

// --- Health Check Endpoint (10.12) ---
app.MapHealthChecks("/health");

// --- Graceful Shutdown (10.9 - Disposability) ---
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
    app.Logger.LogInformation("[AuthService] Uygulama kapatiliyor..."));
lifetime.ApplicationStopped.Register(() =>
    app.Logger.LogInformation("[AuthService] Uygulama tamamen kapatildi."));

// --- Veritabanını otomatik oluştur + Seed Data ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
