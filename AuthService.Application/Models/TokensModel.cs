namespace AuthService.Application.Models;

public record TokensModel
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;

    public static TokensModel Create(string accessToken, string refreshToken) =>
        new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
}