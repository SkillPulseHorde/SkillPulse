using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public sealed record AssessmentResultResponseDto(
    Dictionary<Guid, CompetenceSummaryDto?> CompetenceSummaries);

public sealed record CompetenceSummaryDto(
    double AverageCompetenceScore,
    Dictionary<Guid, CriterionSummaryDto> CriterionSummaries,
    List<string> CompetenceComments);

public sealed record CriterionSummaryDto(
    double AverageCriterionScore,
    List<string> CriterionComments);


public static class AssessmentResultDtoExtensions
{
    public static AssessmentResultResponseDto ToDto(this AssessmentResultModel model)
    {
        var competenceDict = model.CompetenceSummaries.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value is null
                ? null
                : new CompetenceSummaryDto(
                    kvp.Value.AverageScore,
                    kvp.Value.CriterionSummaries.ToDictionary(
                        c => c.Key,
                        c => new CriterionSummaryDto(
                            c.Value.Score, 
                            [..c.Value.Comments])
                    ),
                    [..kvp.Value.Comments]
                )
        );

        return new AssessmentResultResponseDto(competenceDict);
    }
}