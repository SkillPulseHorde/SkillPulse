using AuthService.Domain.Entities;

namespace AuthService.Application.interfaces;

public interface IJwtProvider
{
    public string GenerateAccessToken(User user);
     
    public string GenerateRefreshToken();
    
    public int GetRefreshExpiresHours();
}