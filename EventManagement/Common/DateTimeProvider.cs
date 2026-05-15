using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagement.Common
{
    /// <summary>
    /// Генератор времени
    /// </summary>
    public static class DateTimeProvider
    {
        /// <summary>
        /// Время UTC с точностью до секунд
        /// </summary>
        public static DateTimeOffset UtcNow 
        {
            get
            {
                var d = DateTimeOffset.UtcNow;

                return new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, 0, d.Offset);
            }
        }
    }
}
