namespace AuthService.Api.DTO;

public record RegistrationRequest
{
    public required string Email { get; init; }
    
    public required string Password { get; init; }
}