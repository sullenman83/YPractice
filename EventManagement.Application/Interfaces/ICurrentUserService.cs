namespace EventManagement.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса получения информации по текущему пользователю
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Проверить пренадлежит ли пользователь роли
    /// </summary>
    /// <param name="role">Название роли</param>
    /// <returns>true - пользователь имеет роль, false - нет</returns>
    bool IsInRole(string role);
}
