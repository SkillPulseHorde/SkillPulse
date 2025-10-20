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

    public async Task<Evaluation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Evaluations
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Guid> CreateAsync(Evaluation evaluation, CancellationToken ct = default)
    {
        evaluation.SubmittedAt = DateTime.UtcNow;
        await _dbContext.Evaluations.AddAsync(evaluation, ct);
        return evaluation.Id;
    }

    public async Task<Guid> UpdateAsync(Evaluation evaluation, CancellationToken ct = default)
    {
        _dbContext.Evaluations.Update(evaluation);
        await _dbContext.SaveChangesAsync(ct);
        return evaluation.Id;
    }
}