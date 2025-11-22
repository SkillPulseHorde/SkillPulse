namespace AssessmentService.Domain.Entities;

/// <summary>
/// Общая оценка пользователя
/// </summary>
public class Evaluation
{
    public const ushort ManagerScoreRatio = 2;
    public const ushort DefaultScoreRatio = 1;
    
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid EvaluatorId { get; init; }
    
    public required ushort RoleRatio { get; init; }
    
    // В рамках какого конкретного периода оценивания сделана оценка
    public required Guid AssessmentId { get; init; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public ICollection<CompetenceEvaluation>? CompetenceEvaluations { get; init; }
    
    public Assessment Assessment { get; init; } = null!;
}