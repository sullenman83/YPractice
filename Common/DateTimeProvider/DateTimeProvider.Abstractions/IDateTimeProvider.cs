using System;
using System.Collections.Generic;
using System.Text;

namespace DateTimeManager.Abstractions
{
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Вернуть текущее utc время 
        /// </summary>
        public DateTimeOffset GetUtcNow();
    }
}
