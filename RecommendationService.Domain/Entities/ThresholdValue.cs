namespace RecommendationService.Domain.Entities;

/// <summary>
/// Пороговые значения по уровням
/// </summary>
public class ThresholdValue
{
    public required string Grade { get; set; }

    public required double MinAvgCompetence { get; set; }

    public required double MinAvgCriterion { get; set; }

    public required double MinCoreThreshold { get; set; }
}