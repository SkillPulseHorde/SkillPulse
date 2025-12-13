namespace AssessmentService.Domain.Entities;

public class Competence
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    
    public required string Description { get; set; }

    public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
}