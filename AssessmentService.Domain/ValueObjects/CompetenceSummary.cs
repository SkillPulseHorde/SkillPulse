namespace AssessmentService.Domain.ValueObjects;

public sealed record CompetenceSummary(
    double AverageScore,
    Dictionary<Guid, CriterionSummary> CriterionSummaries,
    List<string> Comments);