namespace RecommendationService.Application.Models;

public record CompetenceModel
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public List<CriterionModel> Criteria { get; set; } = [];
}

public record CriterionModel
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required bool IsMandatory { get; set; }
}