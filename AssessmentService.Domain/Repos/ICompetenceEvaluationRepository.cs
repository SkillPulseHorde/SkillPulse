using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface ICompetenceEvaluationRepository
{
    Task<CompetenceEvaluation?> GetCompetenceEvaluationReadOnlyByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<Guid> CreateCompetenceEvaluationAsync(CompetenceEvaluation competenceEvaluation, CancellationToken ct = default);
}