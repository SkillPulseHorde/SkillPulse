using RecommendationService.Application.Models;

namespace RecommendationService.Application.ServiceClientsAbstract;

public interface IAssessmentServiceClient
{
    Task<AssessmentResultModel?> GetAssessmentResult(Guid assessmentId, CancellationToken ct = default);

    Task<List<CompetenceModel>> GetCompetenciesByEvaluateeId(Guid evaluateeId, CancellationToken ct = default);
}