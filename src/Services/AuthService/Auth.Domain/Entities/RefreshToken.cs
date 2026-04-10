namespace Auth.Domain.Entities;

/// <summary>
/// Refresh token bilgisi. Token string, expiry date ve userId içerir.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
}
