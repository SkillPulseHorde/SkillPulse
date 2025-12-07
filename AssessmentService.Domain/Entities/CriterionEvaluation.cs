namespace AssessmentService.Domain.Entities;

public class CriterionEvaluation
{
    public const int MinScore = 1;
    public const int MaxScore = 10;

    public required Guid Id { get; init; }

    public required Guid CriterionId { get; init; }

    // В рамках какой конкретной оцениваемой компетенции сделана оценка этого критерия
    public required Guid CompetenceEvaluationId { get; init; }

    public int? Score { get; init; }

    public string? Comment { get; init; }
}