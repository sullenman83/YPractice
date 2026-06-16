using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Конфигуратор бронирований
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <summary>
    /// Сконфигурировать сущность
    /// </summary>
    /// <param name="builder">Конфигуратор сущности</param>
    /// <exception cref="NotImplementedException"></exception>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings")            
            .ToTable(t => t.HasCheckConstraint("chk_bookings_status", "status IN('Pending', 'Confirmed', 'Rejected', 'Cancelled')"))
            .ToTable(t => t.HasCheckConstraint("chk_bookings_seats_count", "seats_count > 0"))
            .HasKey(b => b.Id);

        builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.SeatsCount)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.ProcessedAt);
        
        builder.Property(p => p.EventId)
            .IsRequired();

        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId);

        builder.HasOne(b => b.User)
            .WithMany(b => b.Bookings)
            .HasForeignKey(b => b.UserId);
    }
}
