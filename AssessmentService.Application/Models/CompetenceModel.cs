namespace AssessmentService.Application.Models;

public sealed record CompetenceModel
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    
    public required string Description { get; init; }
    public List<CriterionModel> Criteria { get; set; } = [];
}

public sealed record CriterionModel
{
    public Guid Id { get; init; }
    public required string Name { get; init; }

    public required bool IsMandatory { get; init; }
}