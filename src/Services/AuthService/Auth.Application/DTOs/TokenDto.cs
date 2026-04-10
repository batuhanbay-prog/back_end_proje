namespace Auth.Application.DTOs;

/// <summary>
/// Token yanıtı. AccessToken, RefreshToken ve Expiration içerir.
/// </summary>
public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
