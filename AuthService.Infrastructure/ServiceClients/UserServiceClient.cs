using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Application.interfaces;
using AuthService.Application.Models;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.ServiceClients;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _internalToken;

    public UserServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalToken = configuration["INTERNAL_TOKEN"]
                         ?? throw new InvalidOperationException("UserService.INTERNAL_TOKEN не найден");
    }

    public async Task<UserModel?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserModel>(cancellationToken: ct);
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken ct = default)
    {
        var request  = new HttpRequestMessage(HttpMethod.Get, $"api/users/{email}/id");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var userId = await response.Content.ReadFromJsonAsync<string?>(cancellationToken: ct);
        
        return Guid.TryParse(userId, out var user) ? user : null;
    }
}