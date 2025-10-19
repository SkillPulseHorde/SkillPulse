using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class CriterionConfiguration : IEntityTypeConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        builder.ToTable("Criteria");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Level)
            .HasConversion<string>();

        builder.HasOne<Competence>()
            .WithMany(c => c.Criteria)
            .HasForeignKey(x => x.CompetenceId);
    }
}

