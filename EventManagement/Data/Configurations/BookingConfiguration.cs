using EventManagement.Models.BookingModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Data.Configurations;

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
            .ToTable(t => t.HasCheckConstraint("chk_bookings_status", "status IN('Pending', 'Confirmed', 'Rejected', 'Processing')"))
            .ToTable(t => t.HasCheckConstraint("chk_bookings_seats_count", "seats_count > 0"))
            .HasKey(b => b.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.SeatsCount)
            .HasColumnName("seats_count")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")            
            .IsRequired();

        builder.Property(p => p.ProcessedAt)
            .HasColumnName("processed_at");
        
        builder.Property(p => p.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId);
    }
}
