using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services;

/// <summary>
/// IAuthService implementasyonu.
/// Login: UserManager ile kullanıcıyı bul, şifreyi kontrol et, token üret, refresh token kaydet.
/// Register: UserManager ile yeni kullanıcı oluştur, role ata.
/// RefreshToken: Refresh token'ı kontrol et, yeni access token üret.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AuthDbContext _context;

    public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, AuthDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new Shared.Common.Exceptions.NotFoundException("Kullanıcı bulunamadı.");

        var result = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!result)
            throw new Shared.Common.Exceptions.ValidationException("Geçersiz şifre.");

        var roles = await _userManager.GetRolesAsync(user);
        var tokenDto = _tokenService.CreateToken(user, roles);

        // Refresh token'ı veritabanına kaydet
        var refreshToken = new RefreshToken
        {
            Token = tokenDto.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return tokenDto;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new Shared.Common.Exceptions.ValidationException("Bu e-posta adresi zaten kayıtlı.");

        var user = new AppUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new Shared.Common.Exceptions.ValidationException(string.Join(", ", errors));
        }

        // Varsayılan olarak "User" rolü ata
        await _userManager.AddToRoleAsync(user, "User");

        return true;
    }

    public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (storedToken == null)
            throw new Shared.Common.Exceptions.ValidationException("Geçersiz refresh token.");

        if (storedToken.ExpiryDate < DateTime.UtcNow)
        {
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
            throw new Shared.Common.Exceptions.ValidationException("Refresh token süresi dolmuş.");
        }

        // Eski token'ı iptal et
        storedToken.IsRevoked = true;

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var newTokenDto = _tokenService.CreateToken(user, roles);

        // Yeni refresh token'ı kaydet
        var newRefreshToken = new RefreshToken
        {
            Token = newTokenDto.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return newTokenDto;
    }
}
