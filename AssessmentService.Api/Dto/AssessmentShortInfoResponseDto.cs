using AssessmentService.Application.Models;

namespace AssessmentService.Api.Dto;

public sealed record AssessmentShortInfoResponseDto
{
    public required Guid AssessmentId { get; init; }
    public required DateTime StartAt { get; init; }
    public required DateTime EndsAt { get; init; }
}

public static class AssessmentShortInfoResponseDtoExtensions
{
    public static AssessmentShortInfoResponseDto ToShortInfoResponseDto(this AssessmentShortInfoModel model) =>
        new()
        {
            AssessmentId = model.Id,
            StartAt = model.StartAt,
            EndsAt = model.EndsAt,
        };
}
