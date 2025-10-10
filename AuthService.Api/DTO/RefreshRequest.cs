namespace AuthService.Api.DTO;

public record RefreshRequest
{
    public required string RefreshToken { get; init; }
};