using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain;

public interface IEvaluationAnalyzer
{
    public Task<AssessmentResult?> GetAssessmentResultByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default);
}