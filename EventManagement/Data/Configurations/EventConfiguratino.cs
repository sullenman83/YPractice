using EventManagement.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Data.Configurations;

/// <summary>
/// Конфигуратор событий
/// </summary>
public class EventConfiguratino : IEntityTypeConfiguration<Event>
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
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(256);

        builder.Property(p => p.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("timestamptz")
            .IsRequired();            

        builder.Property(p => p.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(p => p.TotalSeats)
            .HasColumnName("total_seats")
            .IsRequired();            

        builder.Property(p => p.AvailableSeats)
            .HasColumnName("available_seats")
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
