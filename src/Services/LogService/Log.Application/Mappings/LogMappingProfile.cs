using AutoMapper;
using Log.Application.DTOs;
using Log.Domain.Entities;

namespace Log.Application.Mappings;

/// <summary>
/// AutoMapper profili — LogEntry ↔ LogEntryDto dönüşümleri.
/// </summary>
public class LogMappingProfile : Profile
{
    public LogMappingProfile()
    {
        CreateMap<LogEntry, LogEntryDto>();
        CreateMap<LogEntryDto, LogEntry>();
    }
}
