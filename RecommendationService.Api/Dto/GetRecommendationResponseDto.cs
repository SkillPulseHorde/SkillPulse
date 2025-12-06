using RecommendationService.Application.Models;

namespace RecommendationService.Api.Dto;

public sealed record GetRecommendationResponseDto
{
    public List<IprCompetenceModel> RecommendationCompetences { get; set; } = [];
}

public sealed record IprCompetenceDto
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

public static class GetRecommendationResponseDtoExtensions
{
    public static GetRecommendationResponseDto ToResponseDto(this RecommendationModel model)
    {
        return new GetRecommendationResponseDto()
        {
            RecommendationCompetences = model.RecommendationCompetences.Select(c => new IprCompetenceModel
            {
                CompetenceName = c.CompetenceName,
                CompetenceReason = c.CompetenceReason,
                WayToImproveCompetence = c.WayToImproveCompetence,
                IsEvaluated = c.IsEvaluated,
                LearningMaterials = c.LearningMaterials.Select(l => new LearningMaterialModel
                {
                    LearningMaterialName = l.LearningMaterialName,
                    LearningMaterialUrl = l.LearningMaterialUrl,
                    LearningMaterialType = l.LearningMaterialType
                }).ToList()
            }).ToList()
        };
    }
}