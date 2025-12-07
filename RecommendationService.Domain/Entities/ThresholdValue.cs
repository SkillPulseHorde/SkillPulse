namespace RecommendationService.Domain.Entities;

/// <summary>
/// Пороговые значения по уровням
/// </summary>
public class ThresholdValue
{
    public required string Grade { get; set; }

    /// <summary>
    /// Минимальное среднее значение компетенции.
    /// </summary>
    /// <remarks>
    /// Если среднее значение по компетенции меньше этого порога, то компетенция не соответствует требованиям уровня.
    /// </remarks>
    public required double MinAvgCompetence { get; set; }

    /// <summary>
    /// Минимальное среднее значение по критериям.
    /// </summary>
    /// <remarks>
    /// Если значение меньше этого порога, то компетенция не соответствует требованиям уровня.
    /// </remarks>
    public required double MinAvgCriterion { get; set; }

    /// <summary>
    /// Минимальный порог по core критерия.
    /// </summary>
    /// <remarks>
    /// Если среднее значение по core критерию компетенции меньше этого порога, то компетенция не соответствует требованиям уровня.
    /// </remarks>
    public required double MinCoreThreshold { get; set; }
}