namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае превышения лимита активных броней
/// </summary>
public class ActiveBookingLimitException: Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public ActiveBookingLimitException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public ActiveBookingLimitException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public ActiveBookingLimitException(string message, Exception inner) : base(message, inner) { }
}
