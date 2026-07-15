using System;
using System.Collections.Generic;
using System.Text;

namespace Bookings.Infrastructure.Settings;

/// <summary>
/// Настройки для продьюсера
/// </summary>
public class BookingProducerSettings
{
    /// <summary>
    /// Адрес сервера кафки
    /// </summary>
    public string BootstrapServers { get; set; } = string.Empty;
}
