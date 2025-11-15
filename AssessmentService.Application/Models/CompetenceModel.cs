namespace AssessmentService.Application.Models;

public class CompetenceModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<CriterionModel> Criteria { get; set; } = new();
}

public class CriterionModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}