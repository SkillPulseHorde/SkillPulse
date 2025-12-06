namespace RecommendationService.Application.Models;

public sealed class AssessmentResultModel
{
    public required Dictionary<Guid, CompetenceSummary?> CompetenceSummaries { get; init; }
}

public sealed record CompetenceSummary(
    double AverageScore,
    Dictionary<Guid, CriterionSummary> CriterionSummaries);

public sealed record CriterionSummary(
    double Score);