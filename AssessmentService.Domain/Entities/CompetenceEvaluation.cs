namespace AssessmentService.Domain.Entities;

/// <summary>
/// Оценка одной конкретной компетенции
/// </summary>
public class CompetenceEvaluation
{
    public required Guid Id { get; init; }

    // Какая компетенция оценивается
    public required Guid CompetenceId { get; init; }

    // В рамках какой общей оценки
    public required Guid EvaluationId { get; init; }

    public required string Comment { get; init; }

    public ICollection<CriterionEvaluation>? CriterionEvaluations { get; init; }
}