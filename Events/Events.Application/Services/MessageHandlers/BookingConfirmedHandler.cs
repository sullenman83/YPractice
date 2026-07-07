using Contracts;
using DateTimeManager.Abstractions;
using Events.Application.Exceptions;
using Events.Application.Interfaces.MessageHandlers;
using Events.Application.Interfaces.Repositories;
using Events.Application.Models.Messages;
using Events.Domain.Exceptions;
using System.Text.Json;
using TransactionManager.Abstractions;

namespace Events.Application.Services.MessageHandlers;

/// <summary>
/// Обработчик BookingConfirmen сообщений
/// </summary>
/// <param name="eventRepository">Репозиторий событий</param>
/// <param name="inboxRepository">inbox репозиторий</param>
/// <param name="transactionService">сервис транзакций</param>
/// <param name="outboxRepository">репозиторий outbox</param>
/// <param name="dateTimeProvider">Провайдер времени</param>
public class BookingConfirmedHandler(IEventRepository eventRepository, 
    IInboxMessageRepository inboxRepository,
    ITransactionService transactionService,
    IOutboxMessageRepository outboxRepository,
    IDateTimeProvider dateTimeProvider) : IBookingConfirmedHandler
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository = outboxRepository;
    private readonly IInboxMessageRepository _inboxMessageRepository = inboxRepository;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    ///<inheritdoc/>
    public async Task HandleMessage(BookingConfirmed message, CancellationToken token)
    {
        try
        {
            var ev = await _eventRepository.GetByIdAsync(message.EventId, token);
            if (ev == null)
                throw new NotFoundException($"Не найдено событие с id = {message.EventId}");

            await using var tr = await _transactionService.BeginTransactionAsync(token);
            if (!ev.TryReserveSeats(message.SeatsCount))
                throw new NoAvailableSeatsException("Недостаточно мест для бронирования.");
            await _eventRepository.SaveChangesAsync();

            await _inboxMessageRepository.AddAsync(new InboxMessage(message.MessageId));

            var m = new EventSeatsReserved(Guid.NewGuid(), message.BookingId, message.EventId, message.UserId);
            var payload = JsonSerializer.Serialize(m);
            await _outboxMessageRepository.AddAsync(new OutboxMessage(message.EventId, nameof(EventSeatsReserved), _dateTimeProvider.GetUtcNow(), payload, 0, false), token);
            await tr.CommitAsync();
        }
        catch(DublicateInsertionException)
        {
            return;
        }
        catch(Exception ex)
        {
            await using var tr = await _transactionService.BeginTransactionAsync();
            await _inboxMessageRepository.AddAsync(new InboxMessage(message.MessageId));
            var m = new EventSeatsNotReserved(Guid.NewGuid(), message.BookingId, message.EventId, message.UserId, ex.Message);
            var payload = JsonSerializer.Serialize(m);
            await _outboxMessageRepository.AddAsync(new OutboxMessage(message.EventId, nameof(EventSeatsReserved), _dateTimeProvider.GetUtcNow(), payload, 0, false), token);
            await tr.CommitAsync(token);
        }
    }
}
