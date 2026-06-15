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
        builder.HasKey(x => x.Id);
        
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
            .IsRequired();

        builder.HasIndex(x => x.Login)
            .IsUnique();

        builder.HasMany(o => o.Bookings)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId);

    }
}
