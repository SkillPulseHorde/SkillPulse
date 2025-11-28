using RecommendationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecommendationService.Infrastructure.Db;

public class RecommendationDbContext : DbContext
{
    public DbSet<IndividualDevelopmentPlan> IndividualDevelopmentPlans => Set<IndividualDevelopmentPlan>();
    public DbSet<LearningMaterial> LearningMaterials => Set<LearningMaterial>();
    
    public RecommendationDbContext(DbContextOptions<RecommendationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecommendationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}