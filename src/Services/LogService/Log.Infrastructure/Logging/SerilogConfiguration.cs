using Microsoft.Extensions.Configuration;
using Serilog;

namespace Log.Infrastructure.Logging;

/// <summary>
/// Serilog + Seq konfigürasyonu.
/// Structured Logging: Console + Seq (http://localhost:5341) + File.
/// Log seviyeleri: INFO, WARNING, ERROR, CRITICAL
/// </summary>
public static class SerilogConfiguration
{
    public static void Configure(IConfiguration configuration, string serviceName)
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: $"logs/{serviceName}-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(
                configuration["Seq:Url"] ?? "http://localhost:5341")
            .CreateLogger();
    }
}
