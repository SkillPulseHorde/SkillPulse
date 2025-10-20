using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Db;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Id);
        
        builder.HasOne(u => u.Manager)
            .WithMany(m => m.Subordinates)
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(u => u.Position)
            .HasConversion<string>() 
            .HasMaxLength(50)
            .HasDefaultValue(Position.NotDefined);
        
        builder.Property(u => u.Grade)
            .HasConversion<string>() 
            .HasMaxLength(20)
            .HasDefaultValue(Grade.NA);
    }
}