using AuthService.Domain.Entities;

namespace AuthService.Application.interfaces;

public interface IJwtProvider
{
    public string GenerateAccessToken(User user, string role);

    public string GenerateRefreshToken();

    public int GetRefreshExpiresHours();
}