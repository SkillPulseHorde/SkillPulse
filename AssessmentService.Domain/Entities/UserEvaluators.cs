namespace AssessmentService.Domain.Entities;

/// <summary>
/// Оцениваемый и оценивающий пользователи
/// </summary>
public record UserEvaluator
{
    public Guid EvaluateeId { get; set; }
    public Guid EvaluatorId { get; set; }
}