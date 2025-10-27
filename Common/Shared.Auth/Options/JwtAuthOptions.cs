using System.ComponentModel.DataAnnotations;

namespace Common.Shared.Auth.Options;

public class JwtAuthOptions
{
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    public bool ValidateIssuer { get; init; } = false;

    public bool ValidateAudience { get; init; } = false;
    
    public TimeSpan ClockSkew { get; init; } =  TimeSpan.Zero;
}