using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IAssessmentRepository
{
    Task<Guid> CreateAsync(Assessment assessment, CancellationToken ct = default);
    
    Task<List<Guid>> GetEvaluatorIdsByUserIdAsync(Guid userId, CancellationToken ct = default);
    
    Task UpdateEvaluatorsForUserAsync(Guid userId, List<Guid> newEvaluatorIds, CancellationToken ct = default);
}