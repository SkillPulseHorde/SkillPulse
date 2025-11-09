using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Application.ServiceClientsAbstract;
using AuthService.Application.Models;
using AuthService.Infrastructure.Http.ServiceClientOptions;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Http.ServiceClients;

public sealed class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _internalToken;

    private const string BaseUrl = "api/users";
    
    public UserServiceClient(HttpClient httpClient, IOptions<UserServiceOptions> options)
    {
        _httpClient = httpClient;
        _internalToken = options.Value.InternalToken;
    }

    public async Task<UserModel?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserModel>(cancellationToken: ct);
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken ct = default)
    {
        using var request  = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{email}/id");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var userId = await response.Content.ReadFromJsonAsync<string?>(cancellationToken: ct);
        
        return Guid.TryParse(userId, out var user) ? user : null;
    }
}