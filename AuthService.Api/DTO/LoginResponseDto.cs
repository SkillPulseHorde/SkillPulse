namespace AuthService.Api.DTO;

public record LoginResponseDto
{
    public required string AccessToken { get; init; }

    public required string UserId { get; init; }
}