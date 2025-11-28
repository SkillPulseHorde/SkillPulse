using RecommendationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RecommendationService.Infrastructure.Db.Configurations;

public class IndividualDevelopmentPlanConfiguration : IEntityTypeConfiguration<IndividualDevelopmentPlan>
{
    public void Configure(EntityTypeBuilder<IndividualDevelopmentPlan> builder)
    {
        builder.ToTable("IndividualDevelopmentPlans");

        builder.HasKey(x => new {x.AssessmentId});
    }
}