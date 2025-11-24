using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AssessmentDbContext _dbContext;

    public AssessmentRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Guid> CreateAsync(Assessment assessment, CancellationToken ct = default)
    {
        assessment.CreatedAt = DateTime.UtcNow;
        await _dbContext.AddAsync(assessment, ct);
        await _dbContext.SaveChangesAsync(ct);
        return assessment.Id;
    }

    public async Task<Assessment?> GetByIdReadonlyAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await _dbContext.Assessments
            .Include(a => a.Evaluators)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assessmentId, ct);
    }
    
    public async Task<Assessment?> GetByIdAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await _dbContext.Assessments
            .Include(a => a.Evaluators)
            .FirstOrDefaultAsync(a => a.Id == assessmentId, ct);
    }

    public async Task<Guid> UpdateAsync(Assessment assessment, CancellationToken ct = default)
    {
        _dbContext.Assessments.Update(assessment);
        await _dbContext.SaveChangesAsync(ct);
        return assessment.Id;
    }

    public async Task<bool> DeleteAsync(Guid assessmentId, CancellationToken ct = default)
    {
        var affected = await _dbContext.Assessments
            .Where(a => a.Id == assessmentId)
            .ExecuteDeleteAsync(ct);

        return affected > 0;
    }

    public async Task<List<Assessment>> GetAssessmentsReadonlyAsync(bool isActive, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var query = _dbContext.Assessments
            .AsNoTracking();
        
        query = isActive
            ? query.Where(a => a.StartAt <= now && a.EndsAt > now)
            : query.Where(a => a.StartAt > now);

        var assessments = await query
            .OrderByDescending(a => a.StartAt)
            .ThenBy(a => a.EndsAt)
            .ToListAsync(ct);

        return assessments;
    }

    public async Task<List<Assessment>> GetActiveAssessmentsByEvaluatorIdReadonlyAsync(Guid evaluatorId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        
        return await _dbContext.Assessments
            .AsNoTracking()
            .Where(a => a.Evaluators.Any(e => e.EvaluatorId == evaluatorId)
                        && a.StartAt <= now 
                        && a.EndsAt > now
                        && a.Evaluations.All(e => e.EvaluatorId != evaluatorId)) // evaluator ещё не оценил
            .OrderBy(a => a.EndsAt)
            .ThenBy(a => a.StartAt)
            .ToListAsync(ct);
    }

    public Task<List<Assessment>> GetCompletedAssessmentsByEvaluateeIdReadonlyAsync(Guid evaluateeId, CancellationToken ct = default)
    {
        return _dbContext.Assessments
            .AsNoTracking()
            .Where(a => a.EvaluateeId == evaluateeId && a.EndsAt <= DateTime.UtcNow)
            .OrderByDescending(a => a.EndsAt)
            .ThenByDescending(a => a.StartAt)
            .ToListAsync(ct);
    }
}