using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Exceptions;
using Shared.Common.Responses;

namespace Shared.Common.Middleware;

/// <summary>
/// Global Exception Handling Middleware.
/// Tüm unhandled exception'ları tek noktada yakalar ve ApiResponse formatında döner.
/// NotFoundException → 404, ValidationException → 400, diğerleri → 500
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Kaynak bulunamadı: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(ex.Message, 404));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validasyon hatası: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(ex.Errors, 400));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Yetkisiz erişim: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Yetkisiz erişim", 401));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Sunucu hatası oluştu", 500));
        }
    }
}
