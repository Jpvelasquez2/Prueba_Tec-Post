using FutbolManager.Application.Features.Posiciones.Dtos;

namespace FutbolManager.Application.Features.Posiciones.Services;

/// <summary>
/// Servicio que calcula la tabla de posiciones a partir de los partidos jugados.
/// </summary>
public interface IPosicionesService
{
    /// <summary>
    /// Calcula y devuelve la tabla de posiciones ordenada según:
    /// PTS desc → DG desc → GF desc → Nombre asc.
    /// </summary>
    /// <remarks>
    /// El criterio head-to-head no se aplica aquí (documentado en scripts/db/README.md).
    /// </remarks>
    Task<IReadOnlyList<PosicionDto>> CalcularAsync(CancellationToken cancellationToken = default);
}
