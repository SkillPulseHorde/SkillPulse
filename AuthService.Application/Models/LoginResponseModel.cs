namespace AuthService.Application.Models;

public class LoginResponseModel
{
    public required TokensModel TokenResponse { get; init; }
    
    public Guid UserId { get; init; }
}