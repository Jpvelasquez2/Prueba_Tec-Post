namespace FutbolManager.Domain.Posiciones;

/// <summary>
/// Entrada al calculador: un partido jugado con su marcador final.
/// </summary>
/// <remarks>
/// El calculador asume que todos los resultados que recibe son partidos
/// efectivamente jugados (no programados, ni suspendidos, ni cancelados).
/// Filtrar por estado es responsabilidad del llamador.
/// </remarks>
/// <param name="EquipoLocalId">Id del equipo que jugó como local.</param>
/// <param name="EquipoVisitanteId">Id del equipo que jugó como visitante.</param>
/// <param name="GolesLocal">Goles anotados por el local (≥ 0).</param>
/// <param name="GolesVisitante">Goles anotados por el visitante (≥ 0).</param>
public sealed record ResultadoPartido(
    int EquipoLocalId,
    int EquipoVisitanteId,
    int GolesLocal,
    int GolesVisitante);
