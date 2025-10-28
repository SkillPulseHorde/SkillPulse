namespace AssessmentService.Domain.ValueObjects;

public sealed record AssessmentResultData(
    double AssessmentScore,
    Dictionary<Guid, CompetenceSummary> CompetenceSummaries);