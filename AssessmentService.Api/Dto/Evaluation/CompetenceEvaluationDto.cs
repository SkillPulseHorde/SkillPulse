namespace AssessmentService.Api.Dto.Evaluation;

public sealed record CompetenceEvaluationDto
{
    public required Guid CompetenceId { get; init; }
    
    public required List<CriterionEvaluationDto> CriterionEvaluations { get; init; }
    
    public required string CompetenceComment { get; init; }
};