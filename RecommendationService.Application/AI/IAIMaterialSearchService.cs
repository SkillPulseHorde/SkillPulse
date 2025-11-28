using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.AI;

/// <summary>
/// Сервис для поиска учебных материалов с помощью AI
/// </summary>
public interface IAIMaterialSearchService
{
    Task<IEnumerable<LearningMaterial>> SearchMaterialsAsync(string competence, CancellationToken ct = default);
}