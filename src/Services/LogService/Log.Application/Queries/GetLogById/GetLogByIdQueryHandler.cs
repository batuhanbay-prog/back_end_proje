using AutoMapper;
using Log.Application.DTOs;
using Log.Application.Interfaces;
using MediatR;
using Shared.Common.Exceptions;

namespace Log.Application.Queries.GetLogById;

/// <summary>
/// GetLogByIdQuery handler — ID ile log kaydı getirir.
/// </summary>
public class GetLogByIdQueryHandler : IRequestHandler<GetLogByIdQuery, LogEntryDto>
{
    private readonly ILogRepository _repository;
    private readonly IMapper _mapper;

    public GetLogByIdQueryHandler(ILogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<LogEntryDto> Handle(GetLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Log kaydı bulunamadı. Id: {request.Id}");

        return _mapper.Map<LogEntryDto>(log);
    }
}
