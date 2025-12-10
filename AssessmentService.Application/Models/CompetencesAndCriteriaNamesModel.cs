namespace AssessmentService.Application.Models;

public sealed record CompetencesAndCriteriaNamesModel
{
    public required Dictionary<Guid, string> CompetenceNames { get; init; }
    public required Dictionary<Guid, string> CriterionNames { get; init; }
}