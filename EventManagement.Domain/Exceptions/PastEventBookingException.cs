namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае бронирования прошедших событий
/// </summary>
public class PastEventBookingException: Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public PastEventBookingException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public PastEventBookingException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public PastEventBookingException(string message, Exception inner) : base(message, inner) { }
}
