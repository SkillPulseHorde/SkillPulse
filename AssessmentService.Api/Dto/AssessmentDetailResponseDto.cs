using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public sealed record AssessmentDetailResponseDto
{
    public required Guid Id { get; init; }
    public required Guid EvaluateeId { get; init; }
    public string? EvaluateeFullName { get; init; }
    public string? EvaluateePosition { get; init; }
    public string? EvaluateeTeamName { get; init; }
    public required DateTime StartAt { get; init; }
    public required DateTime EndsAt { get; init; }
    public required List<Guid> EvaluatorIds { get; init; }
}

public static class AssessmentDetailResponseDtoExtensions
{
    public static AssessmentDetailResponseDto ToDetailResponseDto(this AssessmentDetailModel model) =>
        new()
        {
            Id = model.Id,
            EvaluateeId = model.EvaluateeId,
            EvaluateeFullName = model.EvaluateeFullName,
            EvaluateePosition = model.EvaluateePosition,
            EvaluateeTeamName = model.EvaluateeTeamName,
            StartAt = model.StartAt,
            EndsAt = model.EndsAt,
            EvaluatorIds = model.EvaluatorIds
        };
}