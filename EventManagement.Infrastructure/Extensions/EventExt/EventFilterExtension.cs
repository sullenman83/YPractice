using EventManagement.Application.Models.FilterModels;
using EventManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Extensions.EventExt;

/// <summary>
/// Класс расширение для событий для фильтрации данных
/// </summary>
internal static class EventFilterExtension
{
    public static IQueryable<Event> Filter(this IQueryable<Event> source, EventFilterRequestDTO filter)
    {        
        if (!string.IsNullOrEmpty(filter.Title))
        {
            var t = filter.Title.ToLower();
            source = source.Where(e => EF.Functions.Like(e.Title.ToLower(), $"%{t}%"));
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

    public static IQueryable<Event> Paginate(this IQueryable<Event> source, EventFilterRequestDTO filter)
    {
        return source.Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);
    }

}
