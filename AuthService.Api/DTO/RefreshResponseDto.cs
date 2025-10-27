namespace AuthService.Api.DTO;

public record RefreshResponseDto()
{
    public required string AccessToken { get; init; }
}