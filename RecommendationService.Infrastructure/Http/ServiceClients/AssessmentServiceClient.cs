using RecommendationService.Application.ServiceClientsAbstract;
using RecommendationService.Infrastructure.Http.ServiceClientOptions;
using RecommendationService.Infrastructure.Dto;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RecommendationService.Application.Models;

namespace RecommendationService.Infrastructure.Http.ServiceClients;

public sealed class AssessmentServiceClient : IAssessmentServiceClient
{
    private readonly string _internalToken;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/api/assessments";

    public AssessmentServiceClient(
        HttpClient httpClient,
        IOptions<AssessmentServiceOptions> options)
    {
        _httpClient = httpClient;
        _internalToken = options.Value.InternalToken;
    }

    public async Task<AssessmentResultModel?> GetAssessmentResult(
        Guid assessmentId,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{assessmentId}/result");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<GetAssessmentResultDto>(cancellationToken: ct);

        return responseBody?.ToModel();
    }

    public async Task<List<CompetenceModel>> GetCompetenciesByEvaluateeId(
        Guid evaluateeId,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/competences/{evaluateeId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<List<GetCompetencesForEvaluateeDto>>(cancellationToken: ct);

        return responseBody?.Select(a => a.ToModel()).ToList() ?? [];
    }
}