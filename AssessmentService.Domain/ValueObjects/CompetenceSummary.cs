namespace AssessmentService.Domain.ValueObjects;

public sealed record CompetenceSummary(
    double AverageScore,
    Dictionary<string, CriterionSummary> CriterionSummaries,
    List<string> Comments);