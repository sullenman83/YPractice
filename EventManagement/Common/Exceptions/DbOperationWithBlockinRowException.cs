namespace EventManagement.Common.Exceptions;


/// <summary>
/// Класс исключение для генерации при возникновении ошибок операций БД с блокировкой строк
/// </summary>
public class DbOperationWithBlockinRowException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public DbOperationWithBlockinRowException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DbOperationWithBlockinRowException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public DbOperationWithBlockinRowException(string message, Exception inner) : base(message, inner) { }
}
