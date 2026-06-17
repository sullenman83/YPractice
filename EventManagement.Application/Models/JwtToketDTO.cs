using EventManagement.Domain.Models;

namespace EventManagement.Application.Models
{
    /// <summary>
    /// DTO класс для передачи данных в генератор JWT токенов
    /// </summary>
    public class JwtToketDTO
    {
        /// <summary>
        /// id пользователя
        /// </summary>
        public required Guid Id { get; set; }
                
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public required string Login { get; set; }

        /// <summary>
        /// Роль пользователя
        /// </summary>
        public required UserRole Role { get; set; }

    }
}
