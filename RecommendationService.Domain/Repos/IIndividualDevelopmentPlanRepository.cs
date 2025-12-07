using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Repos;

public interface IIndividualDevelopmentPlanRepository
{
    Task<IndividualDevelopmentPlan?> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default);

    Task<Guid> CreateAsync(IndividualDevelopmentPlan individualDevelopmentPlan, CancellationToken ct = default);
}