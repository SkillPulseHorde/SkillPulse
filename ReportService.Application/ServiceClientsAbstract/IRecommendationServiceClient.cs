using ReportService.Application.Models;

namespace ReportService.Application.ServiceClientsAbstract;

public interface IRecommendationServiceClient
{
    Task<RecommendationsModel> GetRecommendationsByAssessmentIdAsync(Guid assessmentId, Guid userId, CancellationToken ct);
}