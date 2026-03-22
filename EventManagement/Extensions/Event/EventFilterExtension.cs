using EventManagement.Models.FilterModels;
using EventManagement.Models.Events;

namespace EventManagement.Extensions;

/// <summary>
/// Класс расширение для событий для фильтрации данных
/// </summary>
internal static class EventFilterExtension
{
    public static IEnumerable<Event> Filter(this IEnumerable<Event> source, EventFilterRequestDTO filter)
    {        
        if (!string.IsNullOrEmpty(filter.Title))
        {            
            source = source.Where(e => e.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.From != null)
        {
            var date = filter.From.Value.Date;
            source = source.Where(e => e.StartAt.Date >= date);
        }

        if (filter.To != null)
        {
            var date = filter.To.Value.Date;
            source = source.Where(e => e.EndAt.Date <= date);
        }

        return source;
    }

    public static IEnumerable<Event> Paginate(this IEnumerable<Event> source, EventFilterRequestDTO filter)
    {
        return source.Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);
    }

}
