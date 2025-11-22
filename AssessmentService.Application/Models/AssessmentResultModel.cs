using AssessmentService.Domain.ValueObjects;

namespace AssessmentService.Application.Models;

public sealed class AssessmentResultModel
{
    public required Dictionary<Guid, CompetenceSummary?> CompetenceSummaries { get; init; }
}