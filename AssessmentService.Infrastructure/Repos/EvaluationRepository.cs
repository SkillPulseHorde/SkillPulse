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

    // todo: Пока не используется, изменится 
    public async Task<Evaluation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Evaluations
            .Include(e => e.Assessment)
            .Include(e => e.CompetenceEvaluations!)
                .ThenInclude(ce => ce.CriterionEvaluations)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Guid> CreateAsync(Evaluation evaluation, CancellationToken ct = default)
    {
        await _dbContext.Evaluations.AddAsync(evaluation, ct);
        await _dbContext.SaveChangesAsync(ct);
        return evaluation.Id;
    }
}