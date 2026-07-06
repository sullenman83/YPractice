using Events.Application.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Events.Infrastructure.Data.Configurations;

/// <summary>
/// Конфигуратор для таблицы outbox
/// </summary>
public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    /// <summary>
    /// Сконфигурировать сущность
    /// </summary>
    /// <param name="builder">Конфигуратор сущности</param>
    /// <exception cref="NotImplementedException"></exception>
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.HasKey(k => k.MessageId);
    }
}
