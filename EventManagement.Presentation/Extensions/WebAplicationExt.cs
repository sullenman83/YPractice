using EventManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Presentation.Extensions;

/// <summary>
/// расширение для WebAplication
/// </summary>
public static class WebAplicationExt
{
    /// <summary>
    /// Применить миграции
    /// </summary>
    /// <param name="app">Экземпляр WebApplication</param>
    /// <returns>Экземпляр WebApplication</returns>
    public static WebApplication ApplyMigration(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        return app;
    }
}
