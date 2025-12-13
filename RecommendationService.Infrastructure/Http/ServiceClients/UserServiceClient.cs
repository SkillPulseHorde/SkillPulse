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
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<UserServiceDto>(cancellationToken: ct);

        return users?.ToShortUserModel();
    }
}