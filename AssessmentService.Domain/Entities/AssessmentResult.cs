using AssessmentService.Domain.ValueObjects;

namespace AssessmentService.Domain.Entities;

public sealed class AssessmentResult
{
    public required Guid AssessmentId { get; init; }

    public required AssessmentResultData Data { get; init; }
}