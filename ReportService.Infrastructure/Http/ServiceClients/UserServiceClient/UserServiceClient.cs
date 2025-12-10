using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.Application.Models;
using ReportService.Application.ServiceClientsAbstract;
using ReportService.Infrastructure.Dto;

namespace ReportService.Infrastructure.Http.ServiceClients.UserServiceClient;

public sealed class UserServiceClient(
    HttpClient httpClient,
    IOptions<UserServiceOptions> options)
    : IUserServiceClient
{
    private readonly string _internalToken = options.Value.InternalToken;
    private const string BaseUrl = "/api/users";

    public async Task<UserModel> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct);
        
        if (user is null)
            throw new HttpRequestException($"Пользователь {userId} не найден");

        return new UserModel
        {
            Id = user.Id,
            FullName = $"{user.LastName} {user.FirstName}{(string.IsNullOrEmpty(user.MidName) ? "" : " " + user.MidName)}",
            Position = user.Position,
            TeamName = user.TeamName,
            Grade = Enum.TryParse<EmployeeGrade>(user.Grade, out var grade) ? grade : EmployeeGrade.NA
        };
    }
}
