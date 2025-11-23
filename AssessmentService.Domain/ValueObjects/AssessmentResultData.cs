namespace AssessmentService.Domain.ValueObjects;

public sealed record AssessmentResultData(
    Dictionary<Guid, CompetenceSummary?> CompetenceSummaries);