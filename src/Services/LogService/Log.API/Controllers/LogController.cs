using Log.Application.DTOs;
using Log.Application.Queries.GetLogById;
using Log.Application.Queries.GetLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Responses;

namespace Log.API.Controllers;

/// <summary>
/// Log Controller — merkezi log sorgulama endpoint'leri.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class LogController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Logları filtreli olarak listeler. Servis adı, seviye ve tarih aralığı ile filtreleme yapılabilir.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<LogEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? serviceName,
        [FromQuery] string? logLevel,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetLogsQuery
        {
            ServiceName = serviceName,
            LogLevel = logLevel,
            From = from,
            To = to
        });
        return Ok(ApiResponse<List<LogEntryDto>>.Ok(result));
    }

    /// <summary>
    /// ID'ye göre tekil log kaydı getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<LogEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LogEntryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetLogByIdQuery { Id = id });
        return Ok(ApiResponse<LogEntryDto>.Ok(result));
    }
}
