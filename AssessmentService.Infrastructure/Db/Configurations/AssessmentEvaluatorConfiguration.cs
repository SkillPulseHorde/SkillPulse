using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class AssessmentEvaluatorConfiguration : IEntityTypeConfiguration<AssessmentEvaluator>
{
    public void Configure(EntityTypeBuilder<AssessmentEvaluator> builder)
    {
        builder.ToTable("AssessmentEvaluators");

        builder.HasKey(x => new { x.AssessmentId, x.EvaluatorId });

        builder.HasOne(x => x.Assessment)
            .WithMany(a => a.Evaluators)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индекс для быстрого поиска всех назначенных аттестаций
        builder.HasIndex(x => x.EvaluatorId);
    }
}
