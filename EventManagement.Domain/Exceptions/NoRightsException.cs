namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае отсутствия прав на операцию
/// </summary>
public class NoRightsException: Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public NoRightsException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public NoRightsException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public NoRightsException(string message, Exception inner) : base(message, inner) { }
}
