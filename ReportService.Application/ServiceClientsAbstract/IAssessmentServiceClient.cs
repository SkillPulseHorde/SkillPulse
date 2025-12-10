using ReportService.Application.Models;

namespace ReportService.Application.ServiceClientsAbstract;

public interface IAssessmentServiceClient
{
    Task<AssessmentResultModel> GetAssessmentResultByIdAsync(Guid assessmentId, CancellationToken ct);

    Task<CompetencesAndCriteriaNamesModel> GetCompetencesAndCriteriaNamesAsync(CancellationToken ct);
}