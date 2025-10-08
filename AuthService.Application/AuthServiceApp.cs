using AuthService.Application.interfaces;

namespace AuthService.Application;

public class AuthServiceApp
{
    private readonly IPasswordHasher _passwordHasher;

    public AuthServiceApp(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }
    
    public Task Register(string userName, string email, string password)
    {
        var hashedPassword = _passwordHasher.GeneratePasswordHash(password);
        
    }
}