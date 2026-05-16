using EventManagement.Models.Events;
using EventManagement.Models.FilterModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventManagement.Interfaces.Reposirories;

/// <summary>
/// Интерфейс базового репозитория 
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IBaseRepository<T>
{
    /// <summary>
    /// Получить объект по id
    /// </summary>
    /// <param name="id">id объекта</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Объект</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Добавить объект
    /// </summary>
    /// <param name="entity">Объект</param>
    /// <param name="token">токен отмены</param>
    /// <returns>Сохраненный объект</returns>
    Task<T> AddAsync(T entity, CancellationToken token = default);


    /// <summary>
    /// Получить все объекты
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Список объектов</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken token = default);

    /// <summary>
    /// Удалить объект по id
    /// </summary>
    /// <param name="id">id события</param>
    /// <param name="token">токен отмены</param>
    /// <returns>true - удаление прошло успешно, false - ошибка при удалении</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken token = default);

    /// <summary>
    /// Получить количество объектов
    /// </summary>
    /// <param name="token">токен отмены</param>
    /// <returns>Количество объектов</returns>
    Task<int> GetCountAsync(CancellationToken token = default);

    /// <summary>
    /// Сохранить данные
    /// </summary>
    /// <param name="token">токен отмены</param>
    Task SaveChangesAsync(CancellationToken token = default);


    /// <summary>
    /// Создать транзакцию
    /// </summary>
    /// <param name="token">Токен оотмены</param>
    /// <returns>Транзакция</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token = default);
}
