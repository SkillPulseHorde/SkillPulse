using System.Net.Http.Headers;
using System.Net.Http.Json;
using AssessmentService.Application.Models;
using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Entities;
using AssessmentService.Infrastructure.Dto;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using Microsoft.Extensions.Options;

namespace AssessmentService.Infrastructure.Http.ServiceClients;

public sealed class UserServiceClient(
    HttpClient httpClient,
    IOptions<UserServiceOptions> options)
    : IUserServiceClient
{
    private readonly string _internalToken = options.Value.InternalToken;
    private const string BaseUrl = "/api/users";
    
    public async Task<bool> AreUsersExistAsync(List<Guid> userIds, CancellationToken ct)
    {
        var requestDto = new CheckUsersExistRequestDto
        {
            UserIds = userIds
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/exist");
        request.Content = JsonContent.Create(requestDto);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);

        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: ct);
    }

    public async Task<List<UserModel>> GetUsersByIdsAsync(List<Guid> userIds, CancellationToken ct)
    {
        var requestDto = new GetUsersByIdsRequestDto
        {
            UserIds = userIds
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/by-ids");
        request.Content = JsonContent.Create(requestDto);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserServiceDto>>(cancellationToken: ct);

        return users?.Select(u => new UserModel
        {
            Id = u.Id,
            FullName = $"{u.LastName} {u.FirstName}{(string.IsNullOrEmpty(u.MidName) ? "" : " " + u.MidName)}",
            Position = u.Position,
            TeamName = u.TeamName,
            Grade = Enum.TryParse<EmployeeGrade>(u.Grade, out var grade) ? grade : EmployeeGrade.NA
        }).ToList() ?? [];
    }
}
