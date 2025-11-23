namespace AssessmentService.Application.Models;

public sealed record AssessmentDetailModel
{
    public Guid Id { get; init; }
    public Guid EvaluateeId { get; init; }
    public required string EvaluateeFullName { get; init; }
    public required string EvaluateePosition { get; init; }
    public string? EvaluateeTeamName { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndsAt { get; init; }
    public List<Guid> EvaluatorIds { get; init; } = [];
}