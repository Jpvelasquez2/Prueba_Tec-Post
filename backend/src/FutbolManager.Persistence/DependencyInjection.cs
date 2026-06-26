using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Persistence.Contexts;
using FutbolManager.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FutbolManager.Persistence;

/// <summary>
/// Registra el DbContext, los repositorios y el UnitOfWork de la capa Persistence.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Cablea el acceso a datos.
    /// </summary>
    /// <param name="services">Contenedor DI.</param>
    /// <param name="configuration">Configuración para leer ConnectionStrings:Default.</param>
    /// <returns>La misma colección para encadenamiento.</returns>
    /// <exception cref="InvalidOperationException">Si no se encuentra la cadena de conexión.</exception>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("La cadena de conexión 'Default' no está configurada en appsettings.");

        // DbContext con SQL Server. Habilitamos retries para tolerar caídas
        // transitorias (timeouts breves, failovers).
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        // Repositorios y Unit of Work — Scoped por request HTTP.
        services.AddScoped<IEquipoRepository, Repositories.EquipoRepository>();
        services.AddScoped<IPartidoRepository, Repositories.PartidoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
