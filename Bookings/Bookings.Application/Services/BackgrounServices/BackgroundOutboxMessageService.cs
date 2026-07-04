using Bookings.Application.AppSettings;
using Bookings.Application.Interfaces;
using Bookings.Domain.Models;
using DateTimeManager.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Bookings.Application.Services.BackgrounServices;

/// <summary>
/// Фоновый сервис для отправки сообщений в Kafka
/// </summary>
public class BackgroundOutboxMessageService: BackgroundService
{
    private readonly ILogger<BackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly BookingProducerSettings _bookingProducerSettings;
    

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="logger">Логер</param>
    /// <param name="serviceFactory">Scope фабрика сервисов</param>
    /// <param name="bookingHandlerSettings">Настройки сервиса</param>
    public BackgroundOutboxMessageService(ILogger<BackgroundService> logger, IServiceScopeFactory serviceFactory, IOptions<BookingProducerSettings> bookingHandlerSettings)
    {
        _logger = logger;
        _serviceFactory = serviceFactory;
        _bookingProducerSettings = bookingHandlerSettings.Value;
    }

    /// <summary>
    /// Метод запуска сервиса
    /// </summary>
    /// <param name="stoppingToken">Токен отмены</param>
    /// <returns>Пустая задача</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый сервис BookingProducer запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
       
                
                await Task.Delay(TimeSpan.FromMilliseconds(_bookingProducerSettings.PollingInterval), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при бронировании события.");            
            }
        }

        _logger.LogInformation("Фоновый сервис обработки бронирований остановлен.");
    }    
}