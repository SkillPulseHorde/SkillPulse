using RecommendationService.Application.Models;

namespace RecommendationService.Application.AiServiceAbstract;

/// <summary>
/// Сервис для генерации ИПР
/// </summary>
public interface IAiRecommendationsGeneratorService
{
    Task<RecommendationsModel> GenerateRecommendationsAsync(List<CompetenceWithResultModel> model, CancellationToken ct = default);
}