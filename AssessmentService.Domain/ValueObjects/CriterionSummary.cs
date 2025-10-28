namespace AssessmentService.Domain.ValueObjects;

public sealed record CriterionSummary(
    double Score,
    List<string> Comments);