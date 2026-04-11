namespace Log.Application.DTOs;

/// <summary>
/// Log kaydı DTO'su — API katmanına veri transferi için.
/// </summary>
public class LogEntryDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
}
