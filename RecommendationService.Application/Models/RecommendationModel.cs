namespace RecommendationService.Application.Models;

public record RecommendationModel
{
    public Guid UserId { get; set; }
    
    public RecommendationCriteria[] RecommendationCriterion { get; set; } =  []; 
}

public record RecommendationCriteria
{
    public required string CriteriaName { get; set; } = string.Empty;
    
    public string CriteriaReason { get; set; } = string.Empty;
    
    public string WayToImproveCriteria { get; set; } = string.Empty;

    public LearningMaterial[] LearningMaterials { get; set; } = [];
}

public record LearningMaterial
{
    public string LearningMaterialName { get; set; } = string.Empty;
    
    public string LearningMaterialUrl { get; set; } = string.Empty;
    
    public string LearningMaterialType { get; set; } = string.Empty;
}