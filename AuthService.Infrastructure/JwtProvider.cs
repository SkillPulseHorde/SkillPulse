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

    public string GenerateAccessToken(User user, string role, string? teamName)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimsIdentity.DefaultRoleClaimType, role),
            new("TeamName", teamName ?? "null")
        ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessExpiresMinutes),
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
    public required string SecretKey { get; set; }
    public required int AccessExpiresMinutes { get; set; }
    public required int RefreshExpiresHours { get; set; }
}
