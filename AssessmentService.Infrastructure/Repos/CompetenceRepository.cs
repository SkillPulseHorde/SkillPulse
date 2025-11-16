using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public class CompetenceRepository : ICompetenceRepository
{
    private readonly AssessmentDbContext _dbContext;

    public CompetenceRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Competence[]> GetAllCompetencesReadOnlyAsync(CancellationToken ct = default)
    {
        return await _dbContext.Competences
            .Include(c => c.Criteria)
            .AsNoTracking()
            .ToArrayAsync(ct);
    }

    public async Task<List<Competence>> GetByIdsAsync(List<Guid> competenceIds, CancellationToken ct = default)
    {
        return await _dbContext.Competences
            .Include(c => c.Criteria)
            .Where(c => competenceIds.Contains(c.Id))
            .ToListAsync(ct);
    }
}
