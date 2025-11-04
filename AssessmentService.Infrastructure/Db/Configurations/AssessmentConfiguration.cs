using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");

        builder.HasKey(x => x.Id);
        
        builder.HasMany(x => x.Evaluations)
            .WithOne(x => x.Assessment)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Evaluators)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        // Для поиска по оцениваемому пользователю за заданный период
        builder.HasIndex(x => new { x.EvaluateeId, x.StartAt, x.EndsAt });
        
        // Для поиска за заданный период
        builder.HasIndex(x => new { x.StartAt, x.EndsAt });
    }
}
