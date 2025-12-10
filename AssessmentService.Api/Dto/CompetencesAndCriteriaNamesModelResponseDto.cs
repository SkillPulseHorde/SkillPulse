using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public sealed class CompetencesAndCriteriaNamesModelResponseDto
{
    public required Dictionary<Guid, string> CompetenceNames { get; set; }

    public required Dictionary<Guid, string> CriterionNames { get; set; }
}

public static class CompetencesAndCriteriaNamesModelExtensions
{
    public static CompetencesAndCriteriaNamesModelResponseDto ToDto(this CompetencesAndCriteriaNamesModel model)
    {
        return new CompetencesAndCriteriaNamesModelResponseDto
        {
            CompetenceNames = model.CompetenceNames,
            CriterionNames = model.CriterionNames
        };
    }
}
