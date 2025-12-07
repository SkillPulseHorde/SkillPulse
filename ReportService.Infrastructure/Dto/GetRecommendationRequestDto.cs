namespace ReportService.Infrastructure.Dto;

public sealed record GetRecommendationRequestDto
{
    public required Guid AssessmentId { get; init; }

    public required Guid UserId { get; init; }
}