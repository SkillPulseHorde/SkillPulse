using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class CriterionEvaluationConfiguration : IEntityTypeConfiguration<CriterionEvaluation>
{
    public void Configure(EntityTypeBuilder<CriterionEvaluation> builder)
    {
        builder.ToTable("CriterionEvaluations");

        builder.HasKey(x => x.Id);

        builder.HasOne<CompetenceEvaluation>()
            .WithMany(ce => ce.CriterionEvaluations)
            .HasForeignKey(x => x.CompetenceEvaluationId);

        builder.HasOne<Criterion>()
            .WithMany()
            .HasForeignKey(x => x.CriterionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}