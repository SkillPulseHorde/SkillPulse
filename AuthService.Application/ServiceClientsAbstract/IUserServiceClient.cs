using AuthService.Application.Models;

namespace AuthService.Application.ServiceClientsAbstract;

public interface IUserServiceClient
{
    Task<UserModel?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    
    Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken ct = default);
}