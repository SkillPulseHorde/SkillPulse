namespace ReportService.Application.Models;

public sealed record AssessmentResultModel
{
    public required Dictionary<Guid, CompetenceSummaryModel?> CompetenceSummaries { get; init; }
}

public sealed record CompetenceSummaryModel
{
    public required Dictionary<Guid, CriterionSummaryModel> CriterionSummaries { get; init; }
    public required List<string> Comments { get; init; }
}

public sealed record CriterionSummaryModel
{
    public required double? Score { get; init; }
    public required List<string> Comments { get; init; }
}
