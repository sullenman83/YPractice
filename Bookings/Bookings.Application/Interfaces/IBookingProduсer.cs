using System;
using System.Collections.Generic;
using System.Text;

namespace Bookings.Application.Interfaces;

/// <summary>
/// продюсер для отправки сообщений в брокер сообшений
/// </summary>
public interface IBookingProduсer
{
    /// <summary>
    /// Опубликовать сообщение
    /// </summary>
    /// <param name="topic">Название топика</param>
    /// <param name="key">Ключ для определения партиции</param>
    /// <param name="value">сообщение</param>
    /// <returns></returns>
    Task Produce(string topic, string key, string value);
}
