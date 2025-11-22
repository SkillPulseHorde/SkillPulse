namespace AssessmentService.Application.Models;

public sealed record AssessmentShortInfoModel
{
    public Guid Id { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndsAt { get; init; }
}