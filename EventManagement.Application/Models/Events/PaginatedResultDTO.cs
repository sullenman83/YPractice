namespace EventManagement.Models.Events;

/// <summary>
/// Класс для возврата результата пагинации
/// </summary>
public class PaginatedResultDTO
{
    /// <summary>
    /// События
    /// </summary>
    public List<EventResponseDto> Events { get; set; } = new List<EventResponseDto>();

    /// <summary>
    /// общее количество событий
    /// </summary>
    public int EventsCount {  get; set; }

    /// <summary>
    /// номер текущей страницы
    /// </summary>
    public int Page {  get; set; }

    /// <summary>
    /// количество элементов на текущей странице
    /// </summary>
    public int EventsCountOnCurrentPage { get; set; }
}
