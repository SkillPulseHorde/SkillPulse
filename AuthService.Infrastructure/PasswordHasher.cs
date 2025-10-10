using AuthService.Application.interfaces;

namespace AuthService.Infrastructure;

public class PasswordHasher : IPasswordHasher
{
    public string GeneratePasswordHash(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    
    public bool IsPasswordValid(string password, string hashedPassword) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
}
