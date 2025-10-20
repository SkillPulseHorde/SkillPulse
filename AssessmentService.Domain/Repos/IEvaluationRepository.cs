using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IEvaluationRepository
{
    Task<Evaluation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<Guid> CreateAsync(Evaluation evaluation, CancellationToken ct = default);
    
    Task<Guid> UpdateAsync(Evaluation evaluation, CancellationToken ct = default);
}