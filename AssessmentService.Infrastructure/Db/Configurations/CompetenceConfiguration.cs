using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class CompetenceConfiguration : IEntityTypeConfiguration<Competence>
{
    public void Configure(EntityTypeBuilder<Competence> builder)
    {
        builder.ToTable("Competences");
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Criteria)
            .WithOne()
            .HasForeignKey(cr => cr.CompetenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}