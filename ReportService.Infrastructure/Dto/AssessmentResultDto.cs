namespace ReportService.Infrastructure.Dto;

public sealed record AssessmentResultDto
{
    public required Dictionary<Guid, CompetenceSummaryDto?> CompetenceSummaries { get; init; }
}

public sealed record CompetenceSummaryDto
{
    public required Dictionary<Guid, CriterionSummaryDto> CriterionSummaries { get; init; }
    public required List<string> CompetenceComments { get; init; }
}

public sealed record CriterionSummaryDto
{
    public required double? AverageCriterionScore { get; init; }
    public required List<string> CriterionComments { get; init; }
}
