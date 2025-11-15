namespace AssessmentService.Api.Dto;

public class CompetenceResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<CriterionResponseDto> Criteria { get; set; } = [];
}

public class CriterionResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
