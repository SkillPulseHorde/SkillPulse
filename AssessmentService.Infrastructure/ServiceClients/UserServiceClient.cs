using System.Net.Http.Json;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Infrastructure.Dto;

namespace AssessmentService.Infrastructure.ServiceClients;

public sealed class UserServiceClient(HttpClient httpClient) : IUserServiceClient
{
    public async Task<bool> UsersExistAsync(List<Guid> userIds, CancellationToken ct)
    {
        var userIdsDto = new CheckUsersExistRequestDto
        {
            UserIds = userIds
        };
        var response = await httpClient.PostAsJsonAsync("/api/users/exist", userIdsDto, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: ct);
    }
}