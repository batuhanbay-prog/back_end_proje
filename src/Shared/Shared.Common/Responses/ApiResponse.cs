using System.Text.Json.Serialization;

namespace Shared.Common.Responses;

/// <summary>
/// Tüm API yanıtları için standart wrapper.
/// Success, Message, Data, Errors alanlarını içerir.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "İşlem başarılı")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> Created(T data, string message = "Kayıt oluşturuldu")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 201
        };
    }

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = string.Join(", ", errors),
            StatusCode = statusCode,
            Errors = errors
        };
    }
}
