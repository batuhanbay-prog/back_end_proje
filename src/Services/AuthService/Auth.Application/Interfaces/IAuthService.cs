using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

/// <summary>
/// Auth işlemlerinin interface'i.
/// Login, Register ve RefreshToken operasyonlarını tanımlar.
/// </summary>
public interface IAuthService
{
    Task<TokenDto> LoginAsync(LoginDto dto);
    Task<bool> RegisterAsync(RegisterDto dto);
    Task<TokenDto> RefreshTokenAsync(string refreshToken);
}
