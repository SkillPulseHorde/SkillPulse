using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IEvaluationRepository
{
    Task<Evaluation[]> GetEvaluationsByAssessmentIdReadonlyAsync(Guid assessmentId, CancellationToken ct = default);
    
    Task<Guid> CreateAsync(Evaluation evaluation, CancellationToken ct = default);
}