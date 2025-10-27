namespace AssessmentService.Api.Dto;

public sealed class UpdateEvaluatorsRequestDto
{
    public List<Guid>? EvaluatorIds { get; init; }
}