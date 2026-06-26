using FutbolManager.Domain.Entities;

namespace FutbolManager.Application.Common.Interfaces;

/// <summary>
/// Puerto de acceso a datos para la entidad <see cref="Partido"/>.
/// </summary>
public interface IPartidoRepository
{
    /// <summary>
    /// Obtiene todos los partidos con sus equipos (local y visitante) cargados,
    /// ordenados por fecha descendente.
    /// </summary>
    Task<IReadOnlyList<Partido>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un partido por id con los equipos cargados, o <c>null</c> si no existe.</summary>
    Task<Partido?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve únicamente los partidos jugados (estado = <see cref="Domain.Enums.EstadoPartido.Jugado"/>),
    /// útil para el cálculo de la tabla de posiciones.
    /// </summary>
    Task<IReadOnlyList<Partido>> GetJugadosAsync(CancellationToken cancellationToken = default);

    /// <summary>Agrega un nuevo partido al contexto.</summary>
    Task AddAsync(Partido partido, CancellationToken cancellationToken = default);

    /// <summary>Marca el partido como eliminado en el contexto.</summary>
    void Remove(Partido partido);
}
