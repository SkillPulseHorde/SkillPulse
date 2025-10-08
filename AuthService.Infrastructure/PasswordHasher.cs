using AuthService.Application.interfaces;

namespace AuthService.Infrastructure;

public class PasswordHasher : IPasswordHasher
{
    // Все таки используем BCrypt. Скачал из NuGet
    public string GeneratePasswordHash(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    
    public bool VerifyHashedPassword(string password, string hashedPassword) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
}
