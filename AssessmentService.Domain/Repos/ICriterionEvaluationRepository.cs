using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface ICriterionEvaluationRepository
{
    Task<CriterionEvaluation?> GetCriterionEvaluationReadOnlyByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<Guid> CreateCriterionEvaluationAsync(CriterionEvaluation criterionEvaluation, CancellationToken ct = default);
}