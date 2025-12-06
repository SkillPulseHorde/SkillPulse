using System.Text.Json.Serialization;
using RecommendationService.Application.Models;

namespace RecommendationService.Infrastructure.Dto;

public sealed record IprResultFromAiDto
{
    [JsonPropertyName("competenceName")]
    public required string CompetenceName { get; init; }

    [JsonPropertyName("competenceReason")]
    public required string CompetenceReason { get; init; }

    [JsonPropertyName("wayToImproveCompetence")]
    public required string WayToImproveCompetence { get; init; }

    public IprCompetenceModel ToIprCompetenceModel()
    {
        return new IprCompetenceModel
        {
            CompetenceName = CompetenceName,
            CompetenceReason = CompetenceReason,
            WayToImproveCompetence = WayToImproveCompetence,
            IsEvaluated = !string.IsNullOrEmpty(WayToImproveCompetence)
        };
    }
}
