using Shared.Common.BaseModels;

namespace Log.Domain.Entities;

/// <summary>
/// Log kaydı entity'si. Merkezi loglama için tüm mikroservislerin loglarını tutar.
/// </summary>
public class LogEntry : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}
