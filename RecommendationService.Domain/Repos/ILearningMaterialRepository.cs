using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Repos;

public interface ILearningMaterialRepository
{
    Task<List<LearningMaterial>?> GetByCompetenceAsync(
        string competence,
        List<string> tags,
        CancellationToken ct = default);

    Task AddRangeAsync(List<LearningMaterial> learningMaterials, CancellationToken ct = default);
}