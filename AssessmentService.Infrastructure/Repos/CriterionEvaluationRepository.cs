using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public sealed class CriterionEvaluationRepository : ICriterionEvaluationRepository
{
    private readonly AssessmentDbContext _dbContext;

    public CriterionEvaluationRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CriterionEvaluation?> GetCriterionEvaluationReadOnlyByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.CriterionEvaluations
            .AsNoTracking()
            //.Include(ce => ce.CompetenceEvaluation)
            //.ThenInclude(c => c.Evaluation)
            .FirstOrDefaultAsync(ce => ce.Id == id, ct);
    }

    public async Task<Guid> CreateCriterionEvaluationAsync(CriterionEvaluation criterionEvaluation, CancellationToken ct = default)
    {
        await _dbContext.CriterionEvaluations.AddAsync(criterionEvaluation, ct);
        return criterionEvaluation.Id;
    }
}
