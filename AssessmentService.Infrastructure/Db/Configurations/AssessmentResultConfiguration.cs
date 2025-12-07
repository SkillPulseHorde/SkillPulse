using System.Text.Json;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Db.Configurations;

public class AssessmentResultConfiguration : IEntityTypeConfiguration<AssessmentResult>
{
    public void Configure(EntityTypeBuilder<AssessmentResult> builder)
    {
        builder.ToTable("AssessmentResults");

        builder.HasKey(x => x.AssessmentId);

        // Настройка сериализации в JSON
        builder.Property(x => x.Data)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<AssessmentResultData>(v, (JsonSerializerOptions?)null)!
            )
            .HasColumnType("jsonb");
    }
}
