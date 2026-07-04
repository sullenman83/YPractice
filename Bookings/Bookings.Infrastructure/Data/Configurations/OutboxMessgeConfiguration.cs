using Bookings.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Data.Configurations;


/// <summary>
/// Конфигуратор для таблицы outbox
/// </summary>
public class OutboxMessgeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Сконфигурировать сущность
    /// </summary>
    /// <param name="builder">Конфигуратор сущности</param>
    /// <exception cref="NotImplementedException"></exception>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(k => k.MessageId);

        builder.Property(p => p.MessageId)
            .ValueGeneratedNever();

        builder.Property(p => p.MessageType)
            .IsRequired();

        builder.Property(p => p.OccuredOn)
            .IsRequired();

        builder.Property(p => p.Payload)
            .IsRequired();

        builder.Property(p => p.Processed)
            .IsRequired();

        builder.Property(p => p.RetryCount)
            .IsRequired();
             
        builder.HasIndex(i => new { i.OccuredOn, i.Processed});
    }
}
