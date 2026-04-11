using Log.Application.DTOs;
using MediatR;

namespace Log.Application.Queries.GetLogById;

/// <summary>
/// ID'ye göre tekil log kaydı getiren query.
/// </summary>
public class GetLogByIdQuery : IRequest<LogEntryDto>
{
    public Guid Id { get; set; }
}
