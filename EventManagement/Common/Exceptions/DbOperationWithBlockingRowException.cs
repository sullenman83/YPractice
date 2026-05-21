namespace EventManagement.Common.Exceptions;


/// <summary>
/// Класс исключение для генерации при возникновении ошибок операций БД с блокировкой строк
/// </summary>
public class DbOperationWithBlockingRowException : Exception
{
    /// <summary>
    /// Конструктор
    /// </summary>
    public DbOperationWithBlockingRowException() : base() { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DbOperationWithBlockingRowException(string message) : base(message) { }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="inner">Обеъкт исключения</param>
    public DbOperationWithBlockingRowException(string message, Exception inner) : base(message, inner) { }
}
