using System;
using System.Collections.Generic;
using System.Text;

namespace Bookings.Application.Exceptions
{
    /// <summary>
    /// Исключение лдя продюсера
    /// </summary>
    public class BookingProducerException : Exception
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public BookingProducerException() : base() { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public BookingProducerException(string message) : base(message) { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="inner">Обеъкт исключения</param>
        public BookingProducerException(string message, Exception inner) : base(message, inner) { }
    }  
}
