using AssessmentService.Domain.Entities;
//using AssessmentService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Infrastructure.Db;

public class AssessmentDbContext : DbContext
{
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<CompetenceEvaluation> CompetenceEvaluations => Set<CompetenceEvaluation>();
    public DbSet<CriterionEvaluation> CriterionEvaluations => Set<CriterionEvaluation>();
    public DbSet<Competence> Competences => Set<Competence>();
    public DbSet<Criterion> Criteria => Set<Criterion>();
    public DbSet<UserEvaluator> UserEvaluators => Set<UserEvaluator>();

    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssessmentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}