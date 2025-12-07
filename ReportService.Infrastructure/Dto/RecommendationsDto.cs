namespace ReportService.Infrastructure.Dto;

public sealed record RecommendationsDto
{
    public List<CompetenceRecommendationDto> CompetenceRecommendations { get; set; } = [];
}

public sealed record CompetenceRecommendationDto
{
    public required string CompetenceName { get; set; } = string.Empty;

    public string CompetenceReason { get; set; } = string.Empty;

    public string WayToImproveCompetence { get; set; } = string.Empty;

    public List<LearningMaterialDto> LearningMaterials { get; set; } = [];

    public bool IsEvaluated { get; set; } = true;
}

public sealed record LearningMaterialDto
{
    public string LearningMaterialName { get; set; } = string.Empty;

    public string LearningMaterialUrl { get; set; } = string.Empty;

    public string LearningMaterialType { get; set; } = string.Empty;
}
