namespace AssessmentService.Domain.Entities;

public class Criterion
{
    public required Guid Id { get; set; } = Guid.NewGuid();

    public required Guid CompetenceId { get; set; }

    public required string Name { get; set; }

    public required CriterionLevel Level { get; set; }
}