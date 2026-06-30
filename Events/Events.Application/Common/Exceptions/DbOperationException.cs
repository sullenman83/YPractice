
namespace EventManagement.Application.Common.Exceptions;

/// <summary>
/// Класс исключение для генерации при возникновении ошибок операций БД 
/// </summary>
public class DbOperationException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public DbOperationException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DbOperationException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public DbOperationException(string message, Exception inner) : base(message, inner) { }
}