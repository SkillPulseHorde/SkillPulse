namespace RecommendationService.Domain.Entities;

/// <summary>
/// Хранение ИПР
/// </summary>
public class IndividualDevelopmentPlan
{
    public Guid AssessmentId { get; set; } // Первичный ключ
    
    public required string SummaryJson { get; set; }
}