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

    public async Task<List<LearningMaterial>> GetByCompetenceAsync(string competence, CancellationToken ct = default)
    {
        return await _context.LearningMaterials
            .Where(m => m.Competence == competence)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, List<LearningMaterial>>> GetByCompetencesAsync(List<string> competences,
        CancellationToken ct = default)
    {
        var materials = await _context.LearningMaterials
            .Where(m => competences.Contains(m.Competence))
            .ToListAsync(ct);
        
        return materials
            .GroupBy(m => m.Competence)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task AddRangeAsync(List<LearningMaterial> learningMaterials, CancellationToken ct = default)
    {
        await _context.LearningMaterials.AddRangeAsync(learningMaterials, ct);
        await _context.SaveChangesAsync(ct);
    }
}