namespace ReportService.Infrastructure.Dto;

public sealed class CompetenceDictionariesDto
{
    public required Dictionary<Guid, string> CompetenceNames { get; init; }

    public required Dictionary<Guid, string> CriterionNames { get; init; }
}
