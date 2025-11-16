using AssessmentService.Application.Models;

namespace AssessmentService.Application.ServiceClientsAbstract;

public interface IUserServiceClient
{
    Task<bool> AreUsersExistAsync(List<Guid> userIds, CancellationToken ct);
    
    Task<List<UserModel>> GetUsersByIdsAsync(List<Guid> userIds, CancellationToken ct);
}