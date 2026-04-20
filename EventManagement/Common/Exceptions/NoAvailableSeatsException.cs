namespace EventManagement.Common.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае нехватки мест при бронировании
/// </summary>
public class NoAvailableSeatsException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public NoAvailableSeatsException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public NoAvailableSeatsException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public NoAvailableSeatsException(string message, Exception inner) : base(message, inner) { }
}
