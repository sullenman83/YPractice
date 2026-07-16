using Contracts;
using Events.Application.Common;
using Events.Application.Exceptions;
using Events.Application.Interfaces;
using Events.Application.Interfaces.MessageHandlers;
using Events.Application.Interfaces.Repositories;
using Events.Application.Models.Messages;
using Events.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using TransactionManager.Abstractions;

namespace Events.Application.Services.MessageHandlers;

/// <summary>
/// Обработчик события BookingCancelled
/// </summary>
/// <param name="eventRepository">Репозиторий событий</param>
/// <param name="inboxRepository">inbox репозиторий</param>
/// <param name="transactionService">сервис транзакций</param>
/// <param name="logger">Логер</param>
/// <param name="cacheService">Кеш</param>

public class BookingCancelledHandler(IEventRepository eventRepository,
    IInboxMessageRepository inboxRepository,
    ITransactionService transactionService,
    ILogger<BookingCancelledHandler> logger,
    ICacheService cacheService

    ) : IBookingCancelledHandler
{
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IInboxMessageRepository _inboxMessageRepository = inboxRepository;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly ILogger<BookingCancelledHandler> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;

    ///<inheritdoc/>
    public async Task HandleMessageAsync(BookingCancelled message, CancellationToken token)
    {
        try
        {
            var ev = await _eventRepository.GetByIdAsync(message.EventId, token);
            if (ev == null)
                throw new NotFoundException($"Не найдено событие с id = {message.EventId}");
            
            await using var tr = await _transactionService.BeginTransactionAsync(token);
            if (!ev.ReleaseSeats(message.SeatsCount))
                throw new SeatsCountMoreThenTotalException("Число доступных мест превышает общее количество мест события.");
            await _eventRepository.SaveChangesAsync();

            await _inboxMessageRepository.AddAsync(new InboxMessage(message.MessageId));
            await tr.CommitAsync();

            await _cacheService.DeleteAsync(CacheKeys.EventKey(ev.Id));
        }
        catch (DublicateInsertionException ex)
        {
            _logger.LogInformation(ex.Message);
            return;
        }
        catch (Exception ex)
        {
            //ToDo: Возможно тут надо кого-то чечрез событие уведомить о проблеме списния мест
            _logger.LogError($"Не удалось освободить места для событие '{message.EventId}', бронирование '{message.BookingId}' по причине '{ex.Message}'");
        }
    }
}
