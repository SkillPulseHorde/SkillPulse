namespace AssessmentService.Domain.Entities;

public record Competence
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }

    public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
}