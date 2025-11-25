using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public class AssessmentResultRepository : IAssessmentResultRepository
{
    private readonly AssessmentDbContext _dbContext;

    public AssessmentResultRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAsync(AssessmentResult assessmentResult, CancellationToken ct = default)
    {
        await _dbContext.AddAsync(assessmentResult, ct);
        await _dbContext.SaveChangesAsync(ct);
        return assessmentResult.AssessmentId;
    }

    public async Task<AssessmentResult?> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await _dbContext.AssessmentResults
            .FirstOrDefaultAsync(ar => ar.AssessmentId == assessmentId, ct);
    }

    public async Task<List<AssessmentResult>> GetByAssessmentIdsAsync(List<Guid> assessmentIds, CancellationToken ct = default)
    {
        return await _dbContext.AssessmentResults
            .Where(ar => assessmentIds.Contains(ar.AssessmentId))
            .ToListAsync(ct);
    }

    public async Task CreateRangeAsync(List<AssessmentResult> assessmentResults, CancellationToken ct = default)
    {
        await _dbContext.AssessmentResults.AddRangeAsync(assessmentResults, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}