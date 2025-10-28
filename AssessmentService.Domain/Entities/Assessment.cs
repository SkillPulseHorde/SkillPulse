namespace AssessmentService.Domain.Entities;

/// <summary>
/// Общая информация об аттестации
/// </summary>
public sealed class Assessment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    // Оцениваемый пользователь
    public required Guid EvaluateeId { get; init; }
    
    public DateTime CreatedAt { get; set; }
    
    public required DateTime StartAt { get; init; }
    
    public required DateTime EndsAt { get; init; }
    
    // Кто запустил аттестацию
    public required Guid CreatedByUserId { get; set; }
    
    // Рецензенты
    public ICollection<AssessmentEvaluator> Evaluators { get; init; } = new List<AssessmentEvaluator>();
    
    public ICollection<Evaluation> Evaluations { get; init; } = new List<Evaluation>();
}