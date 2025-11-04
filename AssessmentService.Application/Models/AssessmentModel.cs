namespace AssessmentService.Application.Models;

public sealed class AssessmentModel
{
    public Guid Id { get; init; }
    public Guid EvaluateeId { get; init; }
    public string? EvaluateeFullName { get; init; }
    public string? EvaluateePosition { get; init; }
    public string? EvaluateeTeamName { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndsAt { get; init; }
}
