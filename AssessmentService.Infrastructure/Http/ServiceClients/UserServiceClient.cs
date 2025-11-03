using System.Net.Http.Json;
using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Infrastructure.Dto;

namespace AssessmentService.Infrastructure.Http.ServiceClients;

public sealed class UserServiceClient(HttpClient httpClient) : IUserServiceClient
{
    public async Task<bool> UsersExistAsync(List<Guid> userIds, CancellationToken ct)
    {
        var requestDto = new CheckUsersExistRequestDto
        {
            UserIds = userIds
        };
        var response = await httpClient.PostAsJsonAsync("/api/users/exist", requestDto, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: ct);
    }

    public async Task<List<UserModel>> GetUsersByIdsAsync(List<Guid> userIds, CancellationToken ct)
    {
        var requestDto = new GetUsersByIdsRequestDto
        {
            UserIds = userIds
        };

        var response = await httpClient.PostAsJsonAsync("/api/users/by-ids", requestDto, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserServiceDto>>(cancellationToken: ct);

        return users?.Select(u => new UserModel
        {
            Id = u.Id,
            FullName = $"{u.LastName} {u.FirstName}{(string.IsNullOrEmpty(u.MidName) ? "" : " " + u.MidName)}",
            Position = u.Position,
            TeamName = u.TeamName
        }).ToList() ?? [];
    }
}
    