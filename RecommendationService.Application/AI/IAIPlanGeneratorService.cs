using Common;

namespace RecommendationService.Application.AI;

/// <summary>
/// Сервис для генерации ИПР
/// </summary>
public interface IAiPlanGeneratorService
{
    Task<Result<string>> GeneratePlanAsync(string assessmentData,
        CancellationToken ct = default);
}