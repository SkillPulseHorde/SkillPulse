using RecommendationService.Domain.Entities;

namespace RecommendationService.Infrastructure.Db;

public static class ThresholdValueSeed
{
    public static readonly IEnumerable<ThresholdValue> Data =
    [
        new ThresholdValue
        {
            Grade = "J1",
            MinAvgCompetence = 5.0,
            MinAvgCriterion = 3,
            MinCoreThreshold = 5.0
        },
        new ThresholdValue
        {
            Grade = "J2",
            MinAvgCompetence = 5.5,
            MinAvgCriterion = 4.0,
            MinCoreThreshold = 6.0
        },
        new ThresholdValue
        {
            Grade = "J3",
            MinAvgCompetence = 6.0,
            MinAvgCriterion = 5,
            MinCoreThreshold = 6.0
        },
        new ThresholdValue
        {
            Grade = "M1",
            MinAvgCompetence = 7.0,
            MinAvgCriterion = 6.0,
            MinCoreThreshold = 7.0
        },
        new ThresholdValue
        {
            Grade = "M2",
            MinAvgCompetence = 7.5,
            MinAvgCriterion = 7.0,
            MinCoreThreshold = 7.0
        },
        new ThresholdValue
        {
            Grade = "M3",
            MinAvgCompetence = 8.0,
            MinAvgCriterion = 7.0,
            MinCoreThreshold = 8.0
        },
        new ThresholdValue
        {
            Grade = "S",
            MinAvgCompetence = 8.5,
            MinAvgCriterion = 8.0,
            MinCoreThreshold = 8.0
        },
    ];
}