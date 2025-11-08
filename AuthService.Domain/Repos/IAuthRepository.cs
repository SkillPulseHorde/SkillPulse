using AuthService.Domain.Entities;

namespace AuthService.Domain.Repos;

public interface IAuthRepository
{
    Task CreateRegistrationAsync(User user, CancellationToken ct = default);
    
    Task<User?> GetUserByEmailReadOnlyAsync(string email, CancellationToken ct = default);
    
    Task UpdateRefreshTokenUserAsync(User user, CancellationToken ct = default);
    
    Task<User?> GetUserByIdReadOnlyAsync(Guid userId, CancellationToken ct = default);
    
    Task<User?> GetUserByRefreshTokenReadOnlyAsync(string requestRefreshToken, CancellationToken ct = default);
}