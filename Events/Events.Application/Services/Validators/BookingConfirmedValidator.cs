using DateTimeManager.Abstractions;
using Events.Application.Interfaces.Validators;
using Events.Domain.Exceptions;
using System.Runtime.CompilerServices;

namespace Events.Application.Services.Validators;

/// <summary>
/// Валидатор подтверждения бронирования и списания мест у события
/// </summary>
public class BookingConfirmedValidator(IDateTimeProvider dateTimeProvider) : IBookingConfirmedValidator
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    ///<inheritdoc/>
    public void ValidateEventDate(DateTimeOffset startDate)
    {
        if (startDate <= _dateTimeProvider.GetUtcNow())
            throw new PastEventBookingException("Нельзя забронировать событие, которое уже началось");
    }
}
