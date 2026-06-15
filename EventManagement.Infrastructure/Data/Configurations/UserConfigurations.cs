using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagement.Infrastructure.Data.Configurations;

internal class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("chk_users_role", "role IN('User', 'Admin')"))
            .ToTable(t => t.HasCheckConstraint("chk_users_loginLen", "LENGTH(login) >= 3"))
            .ToTable(t => t.HasCheckConstraint("chk_users_passwordLen", "LENGTH(password) > 0"))
            .HasKey(x => x.Id);
        
        builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.Login)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(p => p.Password)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(x => x.Login)
            .IsUnique();

        builder.HasMany(o => o.Bookings)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId);

    }
}
