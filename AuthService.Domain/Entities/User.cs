namespace AuthService.Domain.Entities;

public class User
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }
    
    public void SetRefreshToken(string refreshToken, DateTimeOffset expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }
}