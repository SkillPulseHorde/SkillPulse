namespace AuthService.Api.DTO;

public record RegisterRequest
{
    public required string Email { get; init; }
    
    public required string Password { get; init; }
}