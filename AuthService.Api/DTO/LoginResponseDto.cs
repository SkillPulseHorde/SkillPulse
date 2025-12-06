using AuthService.Application.Models;

namespace AuthService.Api.DTO;

public sealed record LoginResponseDto
{
    public required string AccessToken { get; init; }

    public required Guid UserId { get; init; }
}

public static class LoginResponseDtoExtensions
{
    public static LoginResponseDto ToResponseDto(this LoginResponseModel model) =>
        new()
        {
            AccessToken = model.TokenResponse.AccessToken,
            UserId = model.UserId
        };
}