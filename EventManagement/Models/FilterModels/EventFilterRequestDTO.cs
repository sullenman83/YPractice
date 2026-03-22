namespace EventManagement.Models.FilterModels;

/// <summary>
/// Фильтр событий
/// </summary>
public class EventFilterRequestDTO
{
    /// <summary>
    /// поиск по названию 
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// события, которые начинаются не раньше указанной даты
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// события, которые заканчиваются не позже указанной даты
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// страница, которую необходимо вернуть
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// количество элементов на странице
    /// </summary>
    public int PageSize { get; set; } = 10;
}
