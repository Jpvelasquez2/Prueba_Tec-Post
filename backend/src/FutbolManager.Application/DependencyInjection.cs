using FutbolManager.Application.Features.Equipos.Services;
using FutbolManager.Application.Features.Partidos.Services;
using FutbolManager.Application.Features.Posiciones.Services;
using FutbolManager.Domain.Posiciones;
using Microsoft.Extensions.DependencyInjection;

namespace FutbolManager.Application;

/// <summary>
/// Punto de entrada para registrar los servicios de la capa Application en el
/// contenedor de inyección de dependencias.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de aplicación: AutoMapper y los servicios CRUD/lógica.
    /// </summary>
    /// <param name="services">Colección de servicios DI.</param>
    /// <returns>La misma colección para encadenamiento fluido.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper: descubre todos los Profile en el ensamblado de Application.
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        // Servicios de aplicación (Service Pattern). Scoped: una instancia por request.
        services.AddScoped<IEquipoService, EquipoService>();
        services.AddScoped<IPartidoService, PartidoService>();
        services.AddScoped<IPosicionesService, PosicionesService>();

        // Calculador puro de tabla de posiciones. Singleton: no tiene estado
        // mutable y su método Calcular es completamente determinístico, así
        // que una sola instancia compartida es segura y económica.
        services.AddSingleton<CalculadorTablaPosiciones>();

        return services;
    }
}
