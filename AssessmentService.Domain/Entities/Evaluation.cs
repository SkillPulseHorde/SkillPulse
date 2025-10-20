namespace AssessmentService.Domain.Entities;

/// <summary>
/// Общая оценка пользователя
/// </summary>
public record Evaluation
{
    public required Guid Id { get; init; } = Guid.NewGuid();

    public required Guid EvaluatorId { get; init; }

    // В рамках какого конкретного периода оценивания сделана оценка
    public required Assessment Assessment { get; init; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public ICollection<CompetenceEvaluation>? CompetenceEvaluations { get; init; }
}