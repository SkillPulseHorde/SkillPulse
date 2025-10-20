using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class CompetenceEvaluationConfiguration : IEntityTypeConfiguration<CompetenceEvaluation>
{
    public void Configure(EntityTypeBuilder<CompetenceEvaluation> builder)
    {
        builder.ToTable("CompetenceEvaluations");
        
        builder.HasKey(x => x.Id);
        
        builder.HasOne<Competence>()
            .WithMany()
            .HasForeignKey(x => x.CompetenceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.CriterionEvaluations)
            .WithOne()
            .HasForeignKey(ce => ce.CompetenceEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}