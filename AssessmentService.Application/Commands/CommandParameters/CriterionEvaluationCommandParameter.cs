namespace AssessmentService.Application.Commands.CommandParameters;

public sealed record CriterionEvaluationCommandParameter
{
    public required Guid CriterionId { get; init; }

    public int? Score { get; init; }

    public string? CriterionComment { get; init; }
}