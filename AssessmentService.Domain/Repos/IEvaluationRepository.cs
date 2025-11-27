using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IEvaluationRepository
{
    /// <summary>
    /// Получить оценки по Id аттестации
    /// </summary>
    Task<Evaluation[]> GetEvaluationsByAssessmentIdReadonlyAsync(Guid assessmentId, CancellationToken ct = default);
    
    /// <summary>
    /// Получить словарь (Id аттестации -> список оценок)
    /// </summary>
    Task<Dictionary<Guid, Evaluation[]>> GetEvaluationsByAssessmentIdsReadonlyAsync(List<Guid> assessmentIds, CancellationToken ct = default);
    
    Task<Guid> CreateAsync(Evaluation evaluation, CancellationToken ct = default);
}