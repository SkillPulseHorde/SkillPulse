using ReportService.Application.Models;

namespace ReportService.Application.ServiceClientsAbstract;

public interface IUserServiceClient
{
    Task<UserModel> GetUserByIdAsync(Guid userId, CancellationToken ct);
}