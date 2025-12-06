namespace AuthService.Application.Models;

public class LoginResponseModel
{
    public required TokensModel TokenResponse { get; init; }

    public Guid UserId { get; init; }

    public static LoginResponseModel Create(Guid userId, TokensModel tokens) =>
        new()
        {
            UserId = userId,
            TokenResponse = tokens
        };
}