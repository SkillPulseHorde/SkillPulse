namespace AssessmentService.Application.Models;

public sealed class AssessmentDetailModel
{
    public Guid Id { get; init; }
    public Guid EvaluateeId { get; init; }
    public string? EvaluateeFullName { get; init; }
    public string? EvaluateePosition { get; init; }
    public string? EvaluateeTeamName { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndsAt { get; init; }
    public List<Guid> EvaluatorIds { get; init; } = [];
}