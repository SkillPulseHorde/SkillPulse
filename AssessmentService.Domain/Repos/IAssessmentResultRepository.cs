using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IAssessmentResultRepository
{
    Task<Guid> CreateAsync(AssessmentResult assessmentResult, CancellationToken ct = default);
    
    Task<AssessmentResult?> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default);
    
    Task<List<AssessmentResult>> GetByAssessmentIdsAsync(List<Guid> assessmentIds, CancellationToken ct = default);
    
    Task CreateRangeAsync(List<AssessmentResult> assessmentResults, CancellationToken ct = default);
}