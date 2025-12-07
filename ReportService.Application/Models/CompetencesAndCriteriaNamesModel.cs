namespace ReportService.Application.Models;

public sealed record CompetencesAndCriteriaNamesModel
{
    public required Dictionary<Guid, string> CompetenceNames { get; set; }
    public required Dictionary<Guid, string> CriterionNames { get; set; }
}