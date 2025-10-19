using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _jwtOptions;
    
    public JwtProvider(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }
    
    public string GenerateAccessToken(User user)
    {
        Claim[] claims = [new("sub", user.Userid.ToString())];
        
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddHours(_jwtOptions.AccessExpiresHours),
            claims: claims);
        
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        
        return tokenValue;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public int GetRefreshExpiresHours()
    {
        return _jwtOptions.RefreshExpiresHours;
    }
}

public class JwtOptions
{
    public string SecretKey { get; set; }
    public int AccessExpiresHours { get; set; }
    public int RefreshExpiresHours { get; set; }
}
