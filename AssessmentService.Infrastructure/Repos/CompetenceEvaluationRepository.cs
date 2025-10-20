using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public sealed class CompetenceEvaluationRepository : ICompetenceEvaluationRepository
{
    private readonly AssessmentDbContext _dbContext;

    public CompetenceEvaluationRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompetenceEvaluation?> GetCompetenceEvaluationReadOnlyByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.CompetenceEvaluations
            .AsNoTracking()
            //.Include(ce => ce.Evaluation)
            .FirstOrDefaultAsync(ce => ce.Id == id, ct);
    }

    public async Task<Guid> CreateCompetenceEvaluationAsync(CompetenceEvaluation competenceEvaluation, CancellationToken ct = default)
    {
        await _dbContext.CompetenceEvaluations.AddAsync(competenceEvaluation, ct);
        return competenceEvaluation.Id;
    }
}
