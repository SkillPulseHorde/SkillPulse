using AuthService.Domain.Entities;

namespace AuthService.Domain.Repos;

public interface IAuthRepository
{
    Task CreateRegistrationAsync(User user, CancellationToken ct = default);
    
    Task<User?> GetUserByEmailReadOnlyAsync(string email, CancellationToken ct = default);
    
    Task UpdateUserAsync(User user, CancellationToken ct = default);
    
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    
    Task<User?> GetUserByRefreshTokenAsync(string requestRefreshToken, CancellationToken ct = default);
}