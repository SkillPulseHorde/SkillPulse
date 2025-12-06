using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Repos;
using RecommendationService.Infrastructure.Db;

namespace RecommendationService.Infrastructure.Repos;

public class LearningMaterialRepository : ILearningMaterialRepository
{
    private readonly RecommendationDbContext _context;

    public LearningMaterialRepository(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LearningMaterial>?> GetByCompetenceAsync(string competence,
        List<string> tags,
        CancellationToken ct = default)
    {
        if (tags.Count == 0)
            return null;

        var cutoff = DateTime.UtcNow.AddMonths(-1);

        var rowMaterials = await _context.LearningMaterials
            .Where(m => competence == m.CompetenceName && m.Created <= cutoff && tags.Contains(m.Tag.ToString()))
            .OrderByDescending(m => m.Created)
            .ToListAsync(ct);

        var materials = rowMaterials.GroupBy(m => m.Tag)
            .Select(g => g.First())
            .ToList();

        return materials.Count == tags.Count ? materials : null;
    }

    public async Task AddRangeAsync(List<LearningMaterial> learningMaterials, CancellationToken ct = default)
    {
        await _context.LearningMaterials.AddRangeAsync(learningMaterials, ct);
        await _context.SaveChangesAsync(ct);
    }
}