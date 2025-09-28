using UserService.Domain.Entities;
using UserService.Domain.Repos;

namespace UserService.Application;

public sealed class UserServiceApp
{
    private readonly IUserRepository _userRepository;

    public UserServiceApp(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct = default)
       => _userRepository.GetUserByIdAsync(id, ct);
    
    public Task<List<User>> GetSubordinatesByUserIdAsync(Guid managerId, CancellationToken ct = default)
        => _userRepository.GetSubordinatesByUserIdAsync(managerId, ct);
}