namespace AssessmentService.Api.Dto.Evaluation;

public sealed record CriterionEvaluationDto
{
    public required Guid CriterionId { get; init; }
    
    public int? Score { get; init; }
    
    public string? CriterionComment { get; init; }
};