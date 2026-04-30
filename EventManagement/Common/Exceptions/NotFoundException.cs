namespace EventManagement.Common.Exceptions;


/// <summary>
/// Класс исключение для генерации если не найден оъект
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public NotFoundException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public NotFoundException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public NotFoundException(string message, Exception inner) : base(message, inner) { }
}
