using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface ICompetenceRepository
{
    Task<Competence[]> GetAllCompetencesReadOnlyAsync(CancellationToken ct = default);

    Task<List<Competence>> GetByIdsAsync(List<Guid> competenceIds, CancellationToken ct = default);

    Task<Dictionary<Guid, List<Guid>>> GetCompetenceCriteriaMapAsync(CancellationToken ct = default);
}