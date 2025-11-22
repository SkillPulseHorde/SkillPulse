using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public sealed class EvaluationRepository : IEvaluationRepository
{
    private readonly AssessmentDbContext _dbContext;

    public EvaluationRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Evaluation[]> GetEvaluationsByAssessmentIdReadonlyAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await _dbContext.Evaluations
            .AsNoTracking()
            .Where(e => e.AssessmentId == assessmentId)
            .Include(e => e.CompetenceEvaluations!)
                .ThenInclude(ce => ce.CriterionEvaluations)
            .ToArrayAsync(ct);
    }

    public async Task<Guid> CreateAsync(Evaluation evaluation, CancellationToken ct = default)
    {
        await _dbContext.Evaluations.AddAsync(evaluation, ct);
        await _dbContext.SaveChangesAsync(ct);
        return evaluation.Id;
    }
}