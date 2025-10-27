namespace AssessmentService.Application.ServiceClientsAbstract;

public interface IUserServiceClient
{
    Task<bool> UsersExistAsync(List<Guid> userIds, CancellationToken ct);
}