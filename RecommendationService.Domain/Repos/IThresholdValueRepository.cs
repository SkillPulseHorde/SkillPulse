using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Repos;

public interface IThresholdValueRepository
{
    Task<ThresholdValue?> GetThresholdValueByGrade(string grade, CancellationToken ct = default);
}