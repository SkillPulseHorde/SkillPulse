namespace AssessmentService.Api.Dto;

public sealed record CreateAssessmentRequestDto
{
    public required Guid EvaluateeId { get; init; }
    public required DateTime StartAt { get; init; }
    public required DateTime EndsAt { get; init; }
    
    public required Guid CreatedByUserId { get; init; }
    
    public required List<Guid> EvaluatorIds { get; init; }
}