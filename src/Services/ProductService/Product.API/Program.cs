using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.Application.Behaviors;
using Product.Application.Interfaces;
using Product.Application.Mappings;
using Product.Infrastructure.Caching;
using Product.Infrastructure.Data;
using Product.Infrastructure.Messaging;
using Product.Infrastructure.Repositories;
using Shared.Common.Middleware;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// --- Ortam değişkenlerinden konfigürasyon (12 Faktör) ---
builder.Configuration.AddEnvironmentVariables();

// --- DbContext (SQL Server) ---
var productConnStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(productConnStr));

// --- Redis ---
var redisConnStr = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnStr));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// --- Health Checks (10.12 - Admin Prosesleri) ---
builder.Services.AddHealthChecks()
    .AddSqlServer(productConnStr, name: "sqlserver", tags: new[] { "db" })
    .AddRedis(redisConnStr, name: "redis", tags: new[] { "cache" });

// --- MediatR + CQRS ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Product.Application.Commands.CreateProduct.CreateProductCommand).Assembly));

// --- FluentValidation + MediatR Pipeline ---
builder.Services.AddValidatorsFromAssembly(typeof(Product.Application.Commands.CreateProduct.CreateProductCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

// --- Repository (DI) ---
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// --- MassTransit (RabbitMQ Event Publisher) ---
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});
builder.Services.AddScoped<IEventBus, MassTransitEventBus>();

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

// --- Authorization ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "Product Mikroservis - CQRS + MediatR + Redis Cache"
    });

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
    app.Logger.LogInformation("[ProductService] Uygulama kapatiliyor..."));
lifetime.ApplicationStopped.Register(() =>
    app.Logger.LogInformation("[ProductService] Uygulama tamamen kapatildi."));

// --- Veritabanını otomatik oluştur ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    for (int retry = 0; retry < 5; retry++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch (Exception) when (retry < 4) { await Task.Delay(4000); }
    }
}

app.Run();
