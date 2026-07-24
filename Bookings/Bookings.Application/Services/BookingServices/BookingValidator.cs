using Bookings.Application.AppSettings;
using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces.BookingServices;
using Bookings.Domain.Exceptions;
using Bookings.Domain.Models;
using DateTimeManager.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookings.Application.Services.BookingServices;

/// <summary>
/// Валидатор бронирований
/// </summary>
public class BookingValidator : IBookingValidator
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly BookingSettings _bookingSettings;
    private readonly ILogger<BookingValidator> _logger;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="dateTimeProvider">Провайдер времени</param>
    /// <param name="settings">Настройки</param>
    public BookingValidator(IDateTimeProvider dateTimeProvider, IOptions<BookingSettings> settings, ILogger<BookingValidator> logger)
    {
        _dateTimeProvider = dateTimeProvider;
        _bookingSettings = settings.Value;
        _logger = logger;
    }

    ///<inheritdoc/>
    public void ValidateActiveBooking(IReadOnlyCollection<Booking> bookings)
    {
        if (bookings.Count >= _bookingSettings.MaxActiveBookingCount)
        {
            _logger.LogWarning("Превышено максимальное количество бронирований ({MaxActiveBookingCount})", _bookingSettings.MaxActiveBookingCount);
            throw new ActiveBookingLimitException("Превышено максимальное количество бронирований");
        }
    }    
}
