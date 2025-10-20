using AssessmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class UserEvaluatorConfiguration : IEntityTypeConfiguration<UserEvaluator>
{
    public void Configure(EntityTypeBuilder<UserEvaluator> builder)
    {
        builder.ToTable("UserEvaluators");
        builder.HasKey(x => new { x.EvaluateeId, x.EvaluatorId });
        
        builder.HasIndex(x => x.EvaluatorId);
        builder.HasIndex(x => x.EvaluateeId);
    }
}

