using UserService.Domain.Entities;

namespace UserService.Domain.Repos;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<User>> GetSubordinatesByUserIdAsync(Guid id, CancellationToken ct = default);
}