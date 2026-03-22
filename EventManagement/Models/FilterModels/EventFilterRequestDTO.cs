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
}
