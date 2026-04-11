using AutoMapper;
using Log.Application.DTOs;
using Log.Application.Interfaces;
using MediatR;

namespace Log.Application.Queries.GetLogs;

/// <summary>
/// GetLogsQuery handler — filtreli log sorgulama.
/// </summary>
public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, List<LogEntryDto>>
{
    private readonly ILogRepository _repository;
    private readonly IMapper _mapper;

    public GetLogsQueryHandler(ILogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<LogEntryDto>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetAllAsync(
            request.ServiceName,
            request.LogLevel,
            request.From,
            request.To);

        return _mapper.Map<List<LogEntryDto>>(logs);
    }
}
