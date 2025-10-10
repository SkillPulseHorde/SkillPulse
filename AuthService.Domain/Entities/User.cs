namespace AuthService.Domain.Entities;

public class User
{
    public required Guid Userid { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
}