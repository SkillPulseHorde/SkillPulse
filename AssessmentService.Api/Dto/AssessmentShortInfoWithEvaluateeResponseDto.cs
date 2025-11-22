using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public sealed record AssessmentShortInfoWithEvaluateeResponseDto
{
    public required Guid AssessmentId { get; init; }
    public required DateTime StartAt { get; init; }
    public required DateTime EndsAt { get; init; }
    public required EvaluateeShortInfoDto EvaluateeInfo { get; init; }
}

public sealed record EvaluateeShortInfoDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Position { get; init; }
    public string? TeamName { get; init; }
}

public static class AssessmentShortInfoWithEvaluateeResponseDtoExtensions
{
    public static AssessmentShortInfoWithEvaluateeResponseDto ToAssessmentShortInfoResponseDto(this AssessmentModel model) =>
        new()
        {
            AssessmentId = model.Id,
            StartAt = model.StartAt,
            EndsAt = model.EndsAt,
            EvaluateeInfo = new EvaluateeShortInfoDto
            {
                Id = model.EvaluateeId,
                FullName = model.EvaluateeFullName,
                Position = model.EvaluateePosition,
                TeamName = model.EvaluateeTeamName
            }
        };
}
