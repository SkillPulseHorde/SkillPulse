namespace AuthService.Api.DTO;

public record LogoutRequest
{
    public required string UserId { get; init; }
};