namespace AssessmentService.Application.Commands.CommandParameters;

public sealed record CompetenceEvaluationCommandParameter
{
    public required Guid CompetenceId { get; init; }
    
    public string? CompetenceComment { get; init; }
    
    public required List<CriterionEvaluationCommandParameter>? CriterionEvaluations { get; init; }
}