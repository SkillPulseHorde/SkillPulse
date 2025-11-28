using RecommendationService.Domain.ValueObjects;

namespace RecommendationService.Domain.Entities;

/// <summary>
/// Список литературы для ИПР
/// </summary>
public class LearningMaterial
{
    public Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public string? Url  { get; set; }
    
    public required string Competence { get; set; }
    
    public LearningMaterialTag Tag { get; set; }
}