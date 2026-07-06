namespace Events.Application.Exceptions;

/// <summary>
/// Класс исключение для генерации при возникновении в констюмере
/// </summary>
public class ConsumerException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public ConsumerException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public ConsumerException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public ConsumerException(string message, Exception inner) : base(message, inner) { }
}
