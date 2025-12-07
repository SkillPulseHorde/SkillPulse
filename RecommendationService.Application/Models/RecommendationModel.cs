namespace RecommendationService.Application.Models;

public sealed record RecommendationsModel
{
    public List<RecommendationsCompetenceModel> RecommendationCompetences { get; set; } = [];

    public static RecommendationsModel Deserialize(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<RecommendationsModel>(json)
               ?? new RecommendationsModel();
    }

    public string Serialize()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}

public sealed record RecommendationsCompetenceModel
{
    public required string CompetenceName { get; set; } = string.Empty;

    public string CompetenceReason { get; set; } = string.Empty;

    public string WayToImproveCompetence { get; set; } = string.Empty;

    public List<LearningMaterialModel> LearningMaterials { get; set; } = [];

    public bool IsEvaluated { get; set; } = true;
}

public sealed record LearningMaterialModel
{
    public string LearningMaterialName { get; set; } = string.Empty;

    public string LearningMaterialUrl { get; set; } = string.Empty;

    public string LearningMaterialType { get; set; } = string.Empty;
}