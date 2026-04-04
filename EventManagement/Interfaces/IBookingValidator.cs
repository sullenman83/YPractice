using EventManagement.Common.Exceptions;

namespace EventManagement.Interfaces
{
    /// <summary>
    /// Интерфейс валидатора бронирования
    /// </summary>
    public interface IBookingValidator
    {
        /// <summary>
        /// Проверка есть ли событие с указанным id 
        /// </summary>
        /// <param name="eventId">Id события</param>
        /// <param name="token">Токен отмены</param>
        /// <exception cref="BookingValidationException"></exception>
        public Task ValidateAsync(Guid eventId, CancellationToken token);
    }
}
