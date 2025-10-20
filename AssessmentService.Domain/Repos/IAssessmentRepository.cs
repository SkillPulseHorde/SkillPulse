using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IAssessmentRepository
{
    Task<Guid> CreateAsync(Assessment assessment, CancellationToken ct = default);
}