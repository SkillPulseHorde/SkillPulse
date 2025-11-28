using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Repos;

public interface ILearningMaterialRepository
{
    Task<List<LearningMaterial>> GetByCompetenceAsync(string competence, CancellationToken ct = default);
    
    Task<Dictionary<string, List<LearningMaterial>>> GetByCompetencesAsync(List<string> competences, CancellationToken ct = default);

    Task AddRangeAsync(List<LearningMaterial> learningMaterials, CancellationToken ct = default);
}