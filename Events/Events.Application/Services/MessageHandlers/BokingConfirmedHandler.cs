using Contracts;
using Events.Application.Interfaces.MessageHandlers;
using Events.Application.Interfaces.Repositories;
using Events.Domain.Exceptions;
using TransactionManager.Abstractions;

namespace Events.Application.Services.MessageHandlers;

/// <summary>
/// Обработчик BookingConfirmen сообщений
/// </summary>
/// <param name="eventRepository">Репозиторий событий</param>
/// <param name="inboxRepository">inbox репозиторий</param>
public class BokingConfirmedHandler(IEventRepository eventRepository, 
    IInboxMessageRepository inboxRepository,
    ITransactionService transactionService
    
    ) : IBookingConfirmedHandler
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IInboxMessageRepository _inboxMessageRepository = inboxRepository;
    private readonly ITransactionService _transactionService = transactionService;


    public async Task HandleMessage(BookingConfirmed message, CancellationToken token)
    {
        var ev = await _eventRepository.GetByIdAsync(message.EventId, token);
        if (ev == null)
            throw new NotFoundException($"Не найдено событие с id = {message.EventId}");
        await using var tr = await _transactionService.BeginTransactionAsync(token);
        if (!ev.TryReserveSeats(message.SeatsCount))
    }
}
