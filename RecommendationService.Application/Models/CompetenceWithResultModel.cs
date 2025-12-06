using Microsoft.AspNetCore.SignalR.Protocol;

namespace RecommendationService.Application.Models;

public sealed record CompetenceWithResultModel
{
    public string CompetenceName { get; set; } = string.Empty;

    public double? CompetenceAvgScore { get; set; } = double.MinValue;

    public bool? IsPassedThreshold { get; set; } = false;

    public List<CriterionWithResultModel> Criteria { get; set; } = [];
}

public sealed record CriterionWithResultModel
{
    public string CriterionName { get; set; } = string.Empty;

    public double CriterionScore { get; set; } = double.MinValue;

    public bool IsPassedThreshold { get; set; } = false;
}