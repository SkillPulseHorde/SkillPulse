using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Repos;

public class UserEvaluatorRepository : IUserEvaluatorRepository
{
    private readonly AssessmentDbContext _dbContext;

    public UserEvaluatorRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Guid>> GetEvaluatorIdsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.UserEvaluators
            .Where(ue => ue.EvaluateeId == userId)
            .Select(a => a.EvaluatorId)
            .ToListAsync(ct);
    }

    // public async Task<List<Guid>> GetUserIdsByEvaluatorIdAsync(Guid userId, CancellationToken ct = default)
    // {
    //     return await _dbContext.UserEvaluators
    //         .Where(ue => ue.EvaluatorId == userId)
    //         .Select(a => a.EvaluateeId)
    //         .ToListAsync(ct);
    // }

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
}