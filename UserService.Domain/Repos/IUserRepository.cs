using UserService.Domain.Entities;

namespace UserService.Domain.Repos;

public interface IUserRepository
{
    Task<User?> GetUserReadonlyByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken ct = default);
    
    Task<List<User>> GetSubordinatesReadonlyByUserIdAsync(Guid id, CancellationToken ct = default);
}