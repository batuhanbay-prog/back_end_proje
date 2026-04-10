using Auth.Application.DTOs;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

/// <summary>
/// Token üretme interface'i.
/// JWT token ve refresh token üretimini tanımlar.
/// </summary>
public interface ITokenService
{
    TokenDto CreateToken(AppUser user, IList<string> roles);
    string GenerateRefreshToken();
}
