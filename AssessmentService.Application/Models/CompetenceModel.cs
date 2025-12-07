namespace AssessmentService.Application.Models;

public sealed record CompetenceModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<CriterionModel> Criteria { get; set; } = new();
}

public sealed record CriterionModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required bool IsMandatory { get; set; }
}