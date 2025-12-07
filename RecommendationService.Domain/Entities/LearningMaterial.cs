namespace RecommendationService.Domain.Entities;

/// <summary>
/// Список литературы для ИПР
/// </summary>
public class LearningMaterial
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Url { get; set; }

    public required string CompetenceName { get; set; }

    public LearningMaterialTag Tag { get; set; }

    public DateTimeOffset Created { get; set; }
}