using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Repos;
using RecommendationService.Infrastructure.Db;

namespace RecommendationService.Infrastructure.Repos;

public class IndividualDevelopmentPlanRepository : IIndividualDevelopmentPlanRepository
{
    private readonly RecommendationDbContext _context;

    public IndividualDevelopmentPlanRepository(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<IndividualDevelopmentPlan?> GetByAssessmentIdAsync(Guid assessmentId,
        CancellationToken ct = default)
    {
        return await _context.IndividualDevelopmentPlans
            .FirstOrDefaultAsync(p => p.AssessmentId == assessmentId, ct);
    }

    public async Task<Guid> CreateAsync(
        IndividualDevelopmentPlan plan, 
        CancellationToken ct = default)
    {
        await _context.IndividualDevelopmentPlans.AddAsync(plan, ct);
        await _context.SaveChangesAsync(ct);
        return plan.AssessmentId;
    }
}