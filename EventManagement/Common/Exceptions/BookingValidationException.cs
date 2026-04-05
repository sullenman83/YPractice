namespace EventManagement.Common.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае неуспешной валидации бронирования
/// </summary>
public class BookingValidationException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public BookingValidationException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public BookingValidationException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public BookingValidationException(string message, Exception inner) : base(message, inner) { }
}

