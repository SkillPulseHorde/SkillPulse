using RecommendationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RecommendationService.Infrastructure.Db.Configurations;

public class LearningMaterialConfiguration : IEntityTypeConfiguration<LearningMaterial>
{
    public void Configure(EntityTypeBuilder<LearningMaterial> builder)
    {
        builder.ToTable("LearningMaterials");

        builder.HasKey(x => x.Id);

        // Для поиска по конкретной компетенции
        builder.HasIndex(x => new { x.Competence });
    }
}