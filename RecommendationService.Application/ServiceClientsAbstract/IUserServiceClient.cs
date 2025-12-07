using RecommendationService.Application.Models;

namespace RecommendationService.Application.ServiceClientsAbstract;

public interface IUserServiceClient
{
    Task<ShortUserModel?> GetUserByIdAsync(Guid userId, CancellationToken ct);
}