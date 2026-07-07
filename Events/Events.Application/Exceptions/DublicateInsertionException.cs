namespace Events.Application.Exceptions;

/// <summary>
/// Исключение для ошибок вставки дубликата в таблицу
/// </summary>
public class DublicateInsertionException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public DublicateInsertionException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DublicateInsertionException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public DublicateInsertionException(string message, Exception inner) : base(message, inner) { }
}
