namespace AuthService.Application.interfaces;

public interface IPasswordHasher
{
    string GeneratePasswordHash(string password);
    
    bool VerifyHashedPassword(string password, string hashedPassword);
}