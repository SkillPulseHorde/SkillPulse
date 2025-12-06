using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RecommendationService.Application.ServiceClientsAbstract;
using RecommendationService.Application.Models;
using RecommendationService.Infrastructure.Dto;
using RecommendationService.Infrastructure.Http.ServiceClientOptions;

namespace RecommendationService.Infrastructure.Http.ServiceClients;

public sealed class UserServiceClient : IUserServiceClient
{
    private readonly string _internalToken;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/api/users";

    public UserServiceClient(
        HttpClient httpClient,
        IOptions<UserServiceOptions> options)
    {
        _httpClient = httpClient;
        _internalToken = options.Value.InternalToken;
    }

    public async Task<ShortUserModel?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var requestDto = new GetUsersByIdsRequestDto
        {
            UserIds = [userId]
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/by-ids");
        request.Content = JsonContent.Create(requestDto);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserServiceDto>>(cancellationToken: ct);

        if (users is null || users.Count == 0)
            return null;

        return users.First().ToShortUserModel();
    }
}