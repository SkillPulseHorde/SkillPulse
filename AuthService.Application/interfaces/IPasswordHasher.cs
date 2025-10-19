namespace AuthService.Application.interfaces;

public interface IPasswordHasher
{
    string GeneratePasswordHash(string password);
    bool IsPasswordValid(string password, string hashedPassword);
}