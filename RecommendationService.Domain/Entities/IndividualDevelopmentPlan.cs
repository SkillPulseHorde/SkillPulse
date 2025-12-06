namespace RecommendationService.Domain.Entities;

/// <summary>
/// Хранение ИПР
/// </summary>
public class IndividualDevelopmentPlan
{
    public required Guid AssessmentId { get; set; }

    public required string SummaryJson { get; set; }
}