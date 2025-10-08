using AuthService.Domain.Entities;

namespace AuthService.Domain.Repos;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken ct = default);
}