namespace AssessmentService.Api.Dto;

public sealed record UpdateAssessmentRequestDto
{
    public required DateTime EndsAt { get; init; }

    public required List<Guid> EvaluatorIds { get; init; }
}