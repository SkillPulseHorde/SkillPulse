using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ReportService.Application.Models;
using ReportService.Application.ServiceClientsAbstract;
using ReportService.Infrastructure.Dto;

namespace ReportService.Infrastructure.Http.ServiceClients.AssessmentServiceClient;

public sealed class AssessmentServiceClient(
    HttpClient httpClient,
    IOptions<AssessmentServiceOptions> options)
    : IAssessmentServiceClient
{
    private readonly string _internalToken = options.Value.InternalToken;
    private const string BaseUrl = "/api/assessments";

    public async Task<AssessmentResultModel> GetAssessmentResultByIdAsync(Guid assessmentId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{assessmentId}/result");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var assessmentDto = await response.Content.ReadFromJsonAsync<AssessmentResultDto>(cancellationToken: ct);
        
        if (assessmentDto is null)
            throw new HttpRequestException($"Результат аттестации {assessmentId} не найден");

        return new AssessmentResultModel
        {
            CompetenceSummaries = assessmentDto.CompetenceSummaries.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value is null 
                    ? null 
                    : new CompetenceSummaryModel
                    {
                        CriterionSummaries = kvp.Value.CriterionSummaries.ToDictionary(
                            c => c.Key,
                            c => new CriterionSummaryModel
                            {
                                Score = c.Value.AverageCriterionScore,
                                Comments = c.Value.CriterionComments
                            }
                        ),
                        Comments = kvp.Value.CompetenceComments
                    }
            )
        };
    }

    public async Task<CompetencesAndCriteriaNamesModel> GetCompetencesAndCriteriaNamesAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/competences-and-criteria-names/");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _internalToken);
        
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var dictionariesDto = await response.Content.ReadFromJsonAsync<CompetenceDictionariesDto>(cancellationToken: ct);
        
        if (dictionariesDto is null)
            throw new HttpRequestException("Не удалось получить словари компетенций и критериев");

        return new CompetencesAndCriteriaNamesModel
        {
            CompetenceNames = dictionariesDto.CompetenceNames,
            CriterionNames = dictionariesDto.CriterionNames
        };
    }
}
