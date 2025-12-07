namespace AssessmentService.Domain.Entities;

/// <summary>
/// Общая информация об аттестации
/// </summary>
public sealed class Assessment
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // Оцениваемый пользователь
    public required Guid EvaluateeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public required DateTime StartAt { get; set; }

    public required DateTime EndsAt { get; set; }

    // Кто запустил аттестацию
    public required Guid CreatedByUserId { get; set; }

    // Рецензенты
    public ICollection<AssessmentEvaluator> Evaluators { get; set; } = new List<AssessmentEvaluator>();

    public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}