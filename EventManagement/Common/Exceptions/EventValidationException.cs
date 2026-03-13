namespace EventManagement.Common.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае неуспешной валидациисобытия
/// </summary>
public class EventValidationException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public EventValidationException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public EventValidationException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public EventValidationException(string message, Exception inner) : base(message, inner) { }
}
