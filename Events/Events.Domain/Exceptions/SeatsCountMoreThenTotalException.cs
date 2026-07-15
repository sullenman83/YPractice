namespace Events.Domain.Exceptions;

/// <summary>
/// Класс исключение для генерации в случае освобождения мест события, когда число доступных мест становится больше общего количества
/// </summary>
public class SeatsCountMoreThenTotalException: Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public SeatsCountMoreThenTotalException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public SeatsCountMoreThenTotalException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public SeatsCountMoreThenTotalException(string message, Exception inner) : base(message, inner) { }
}
