using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text;
using Log.Application.Interfaces;
using Log.Application.Mappings;
using Log.Application.Queries.GetLogs;
using Log.Infrastructure.Consumers;
using Log.Infrastructure.Data;
using Log.Infrastructure.Logging;
using Log.Infrastructure.Repositories;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Ortam değişkenlerinden konfigürasyon (12 Faktör) ---
builder.Configuration.AddEnvironmentVariables();

// --- Serilog (Structured Logging: Console + File + Seq) ---
SerilogConfiguration.Configure(builder.Configuration, "LogService");
builder.Host.UseSerilog();

// --- DbContext (SQL Server) ---
var logConnStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<LogDbContext>(options =>
    options.UseSqlServer(logConnStr));

// --- Health Checks (10.12 - Admin Prosesleri) ---
builder.Services.AddHealthChecks()
    .AddSqlServer(logConnStr, name: "sqlserver", tags: new[] { "db" });

// --- MediatR + CQRS ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetLogsQuery).Assembly));

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(LogMappingProfile));

// --- Repository (DI) ---
builder.Services.AddScoped<ILogRepository, LogRepository>();

// --- MassTransit (RabbitMQ Consumer) ---
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedEventConsumer>();
    x.AddConsumer<ProductUpdatedEventConsumer>();

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
        Title = "Log API",
        Version = "v1",
        Description = "Log Mikroservis — Merkezi Structured Logging (Serilog + Seq)"
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

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// --- Global Exception Middleware ---
app.UseMiddleware<GlobalExceptionMiddleware>();

// --- Serilog request logging ---
app.UseSerilogRequestLogging();

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
    Serilog.Log.Information("[LogService] Uygulama kapatiliyor..."));
lifetime.ApplicationStopped.Register(() =>
{
    Serilog.Log.Information("[LogService] Uygulama tamamen kapatildi.");
    Serilog.Log.CloseAndFlush();
});

// --- Veritabanını otomatik oluştur ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LogDbContext>();
    for (int retry = 0; retry < 5; retry++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch (Exception) when (retry < 4) { await Task.Delay(4000); }
    }
}

app.Run();
