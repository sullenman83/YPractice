using EventManagement.Application.Common.AppSettings;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Interfaces.Services.BookingServices;
using EventManagement.Domain.Exceptions;
using EventManagement.Domain.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagement.Application.Services.BookingServices;

/// <summary>
/// Валидатор бронирований
/// </summary>
public class BookingValidator : IBookingValidator
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly BookingSettings _bookingSettings;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="dateTimeProvider">Провайдер времени</param>
    /// <param name="settings">Настройки</param>
    public BookingValidator(IDateTimeProvider dateTimeProvider, IOptions<BookingSettings> settings)
    {
        _dateTimeProvider = dateTimeProvider;
        _bookingSettings = settings.Value;
    }

    ///<inheritdoc/>
    public void ValidateActiveBooking(IReadOnlyCollection<Booking> bookings)
    {
        if (bookings.Count >= _bookingSettings.MaxActiveBookingCount)
            throw new ActiveBookingLimitException($"Превышено максимальное количество бронирований ({_bookingSettings.MaxActiveBookingCount})");
    }

    ///<inheritdoc/>
    public void ValidateEventDate(DateTimeOffset startDate)
    {
        if (startDate <= _dateTimeProvider.GetUtcNow())
            throw new PastEventBookingException("Нельзя забронировать событие, которое уже началось");
    }
}
