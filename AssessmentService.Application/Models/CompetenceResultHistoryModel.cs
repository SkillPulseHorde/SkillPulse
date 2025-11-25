namespace AssessmentService.Application.Models;

public sealed class CompetenceResultHistoryModel
{
    public required Guid AssessmentId { get; init; }
    public required DateTime AssessmentDate { get; init; }
    public required double CompetenceScore { get; init; }
}