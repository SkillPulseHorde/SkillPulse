using RecommendationService.Application.Models;

namespace RecommendationService.Infrastructure.Dto;

public sealed record GetAssessmentResultDto(
    Dictionary<Guid, CompetenceSummaryDto?> CompetenceSummaries)
{
    public AssessmentResultModel ToModel()
    {
        var competenceSummaries = CompetenceSummaries.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value is null
                ? null
                : new CompetenceSummary(
                    kvp.Value. AverageCompetenceScore,
                    kvp.Value.CriterionSummaries.ToDictionary(
                        c => c.Key,
                        c => new CriterionSummary(
                            c.Value.AverageCriterionScore)
                    )
                )
        );

        return new AssessmentResultModel
        {
            CompetenceSummaries = competenceSummaries
        };
    }
}

public sealed record CompetenceSummaryDto(
    double AverageCompetenceScore,
    Dictionary<Guid, CriterionSummaryDto> CriterionSummaries,
    List<string> CompetenceComments);

public sealed record CriterionSummaryDto(
    double AverageCriterionScore,
    List<string> CriterionComments);