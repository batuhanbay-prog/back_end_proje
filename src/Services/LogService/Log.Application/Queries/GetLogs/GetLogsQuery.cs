using Log.Application.DTOs;
using MediatR;

namespace Log.Application.Queries.GetLogs;

/// <summary>
/// Logları tarih, seviye ve servis adı ile filtreleyerek getiren query.
/// </summary>
public class GetLogsQuery : IRequest<List<LogEntryDto>>
{
    public string? ServiceName { get; set; }
    public string? LogLevel { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
