using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Repos;
using RecommendationService.Infrastructure.Db;

namespace RecommendationService.Infrastructure.Repos;

public class ThresholdValueRepository : IThresholdValueRepository
{
    private readonly RecommendationDbContext _context;

    public ThresholdValueRepository(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<ThresholdValue?> GetThresholdValueByGrade(string grade,
        CancellationToken ct = default)
    {
        return await _context.ThresholdValues
            .AsNoTracking()
            .FirstOrDefaultAsync(th => th.Grade == grade, ct);
    }
}