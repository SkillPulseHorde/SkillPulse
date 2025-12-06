using RecommendationService.Application.Models;

namespace RecommendationService.Application.AiServiceAbstract;

/// <summary>
/// Сервис для генерации ИПР
/// </summary>
public interface IAiIprGeneratorService
{
    Task<RecommendationModel?> GenerateIprAsync(List<CompetenceWithResultModel> model, CancellationToken ct = default);
}