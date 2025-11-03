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

    public async Task<List<Guid>> GetEvaluatorIdsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserEvaluators
            .Where(ue => ue.EvaluateeId == userId)
            .Select(a => a.EvaluatorId)
            .ToListAsync(ct);
    }

    public async Task UpdateEvaluatorsForUserAsync(Guid userId, List<Guid> newEvaluatorIds, CancellationToken ct = default)
    {
        var existing = await _dbContext.UserEvaluators
            .Where(ue => ue.EvaluateeId == userId)
            .ToListAsync(ct);

        _dbContext.UserEvaluators.RemoveRange(existing);
        
        var linksToAdd = newEvaluatorIds
            .Distinct()
            .Select(evaluatorId => new UserEvaluator
            {
                EvaluateeId = userId,
                EvaluatorId = evaluatorId
            });

        await _dbContext.UserEvaluators.AddRangeAsync(linksToAdd, ct);

        await _dbContext.SaveChangesAsync(ct);
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
}