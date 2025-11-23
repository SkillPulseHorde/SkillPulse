namespace AssessmentService.Api.Dto.Evaluation;

public sealed record CompetenceEvaluationDto
{
    public required Guid CompetenceId { get; init; }
    
    public List<CriterionEvaluationDto>? CriterionEvaluations { get; init; }
    
    public string? CompetenceComment { get; init; }
};