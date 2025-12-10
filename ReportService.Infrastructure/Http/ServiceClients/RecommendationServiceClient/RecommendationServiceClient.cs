using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.Application.Models;
using ReportService.Application.ServiceClientsAbstract;
using ReportService.Infrastructure.Dto;

namespace ReportService.Infrastructure.Http.ServiceClients.RecommendationServiceClient;

public sealed class RecommendationServiceClient(
    HttpClient httpClient,
    IOptions<RecommendationServiceOptions> options)
    : IRecommendationServiceClient
{
    private readonly string _internalToken = options.Value.InternalToken;
    private const string BaseUrl = "/api/recommendations";

    public async Task<RecommendationsModel> GetRecommendationsByAssessmentIdAsync(
        Guid assessmentId,
        Guid userId, 
        CancellationToken ct)
    {
        var body = new GetRecommendationRequestDto
        {
            AssessmentId = assessmentId,
            UserId = userId
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
        request.Content = JsonContent.Create(body);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);

        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var recommendations = await response.Content.ReadFromJsonAsync<RecommendationsModel>(cancellationToken: ct);

        return recommendations ?? throw new HttpRequestException($"Рекомендации для аттестации {assessmentId} не найдены");
    }
}
