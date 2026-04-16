using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Domain.Entities;
using Auth.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Auth.UnitTests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly Mock<IConfiguration> _mockConfig;

    public TokenServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["JwtSettings:SecretKey"]).Returns("TestSecretKeyEnAz32KarakterOlmaliDir!");
        _mockConfig.Setup(c => c["JwtSettings:Issuer"]).Returns("MicroserviceProject");
        _mockConfig.Setup(c => c["JwtSettings:Audience"]).Returns("MicroserviceProject");
        _mockConfig.Setup(c => c["JwtSettings:TokenExpirationInMinutes"]).Returns("60");

        _tokenService = new TokenService(_mockConfig.Object);
    }

    [Fact]
    public void CreateToken_ValidUser_ShouldNotReturnNull()
    {
        var user = new AppUser { Id = Guid.NewGuid().ToString(), UserName = "testuser", Email = "test@test.com" };
        var roles = new List<string> { "User" };

        var result = _tokenService.CreateToken(user, roles);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateToken_ValidUser_ShouldContainCorrectClaims()
    {
        var userId = Guid.NewGuid().ToString();
        var user = new AppUser { Id = userId, UserName = "admin", Email = "admin@admin.com" };
        var roles = new List<string> { "Admin" };

        var result = _tokenService.CreateToken(user, roles);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.AccessToken);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.NameId && c.Value == userId);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "admin@admin.com");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public void CreateToken_ValidUser_ShouldHaveCorrectExpiration()
    {
        var user = new AppUser { Id = Guid.NewGuid().ToString(), UserName = "testuser", Email = "test@test.com" };
        var roles = new List<string> { "User" };

        var before = DateTime.UtcNow.AddMinutes(59);
        var result = _tokenService.CreateToken(user, roles);
        var after = DateTime.UtcNow.AddMinutes(61);

        result.Expiration.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }
}
