using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.AssessmentId, x.EvaluatorId })
            .IsUnique();

        builder.HasOne(x => x.Assessment)
            .WithMany(x => x.Evaluations)
            .HasForeignKey(x => x.AssessmentId);

        builder.HasMany(x => x.CompetenceEvaluations)
            .WithOne()
            .HasForeignKey(ce => ce.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}