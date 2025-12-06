using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecommendationService.Domain.Entities;

namespace RecommendationService.Infrastructure.Db.Configurations;

public class ThresholdValueConfiguration : IEntityTypeConfiguration<ThresholdValue>
{
    public void Configure(EntityTypeBuilder<ThresholdValue> builder)
    {
        builder.ToTable("ThresholdValues");

        builder.HasKey(x => x.Grade);

        builder.HasData(ThresholdValueSeed.Data);
    }
}