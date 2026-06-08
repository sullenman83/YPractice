using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Конфигуратор событий
/// </summary>
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    /// <summary>
    /// Сконфигурировать сущность
    /// </summary>
    /// <param name="builder">Конфигуратор сущности</param>
    /// <exception cref="NotImplementedException"></exception>
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events")
            .ToTable(t => t.HasCheckConstraint("chk_events_end_at", "end_at >= start_at"))
            .ToTable(t => t.HasCheckConstraint("chk_events_total_seats", "total_seats > 0"))
            .ToTable(t => t.HasCheckConstraint("chk_events_available_seats", "available_seats <= total_seats"))
            .ToTable(t => t.HasCheckConstraint("chk_events_title", "LENGTH(title) > 0"))            
            .HasKey(e => e.Id);
            
        builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(256);

        builder.Property(p => p.StartAt)
            .IsRequired();            

        builder.Property(p => p.EndAt)
            .IsRequired();

        builder.Property(p => p.TotalSeats)
            .IsRequired();            

        builder.Property(p => p.AvailableSeats)
            .IsRequired();

        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(k => k.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Title);
        builder.HasIndex(e => e.StartAt);
        builder.HasIndex(e => e.EndAt);
    }
}
