using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Userid);
        
        builder.HasIndex(x => x.Email)
            .IsUnique();
        
        builder.HasIndex(x => x.RefreshToken)
            .IsUnique();
    }
}