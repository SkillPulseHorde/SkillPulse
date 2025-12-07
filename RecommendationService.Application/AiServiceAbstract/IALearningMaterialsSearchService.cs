using RecommendationService.Application.Models;

namespace RecommendationService.Application.AiServiceAbstract;

/// <summary>
/// Сервис для поиска учебных материалов с помощью AI
/// </summary>
public interface IALearningMaterialsSearchService
{
    Task<List<LearningMaterialModel>> SearchLearningMaterialsAsync(
        string competence,
        List<string> tags,
        CancellationToken ct = default);
}