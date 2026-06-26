using FutbolManager.Application.Features.Equipos.Dtos;

namespace FutbolManager.Application.Features.Equipos.Services;

/// <summary>
/// Servicio de aplicación (Service Pattern) para el CRUD de equipos.
/// Orquesta repositorio + UoW + validaciones de negocio.
/// </summary>
public interface IEquipoService
{
    /// <summary>Lista todos los equipos.</summary>
    Task<IReadOnlyList<EquipoDto>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un equipo por id. Lanza <c>NotFoundException</c> si no existe.</summary>
    Task<EquipoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Crea un equipo nuevo y devuelve el DTO resultante.</summary>
    Task<EquipoDto> CrearAsync(CrearEquipoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Actualiza los datos de un equipo existente.</summary>
    Task<EquipoDto> ActualizarAsync(int id, ActualizarEquipoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Elimina un equipo. Falla si tiene partidos asociados (ConflictException).</summary>
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
