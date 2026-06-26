using FutbolManager.Domain.Enums;

namespace FutbolManager.Application.Features.Partidos.Dtos;

/// <summary>
/// Representación pública de un partido. Expone los nombres de los equipos
/// para evitar al consumidor un segundo round-trip.
/// </summary>
/// <param name="Id">Identificador único.</param>
/// <param name="Fecha">Fecha y hora del partido.</param>
/// <param name="LocalTeamId">Id del equipo local.</param>
/// <param name="LocalTeamNombre">Nombre del equipo local.</param>
/// <param name="VisitanteTeamId">Id del equipo visitante.</param>
/// <param name="VisitanteTeamNombre">Nombre del equipo visitante.</param>
/// <param name="LocalScore">Goles del local (null si aún no se jugó).</param>
/// <param name="VisitanteScore">Goles del visitante (null si aún no se jugó).</param>
/// <param name="Estado">Estado actual del partido.</param>
/// <param name="CreatedAt">Fecha UTC de creación.</param>
/// <param name="UpdatedAt">Fecha UTC de última modificación.</param>
public record PartidoDto(
    int Id,
    DateTime Fecha,
    int LocalTeamId,
    string LocalTeamNombre,
    int VisitanteTeamId,
    string VisitanteTeamNombre,
    byte? LocalScore,
    byte? VisitanteScore,
    EstadoPartido Estado,
    DateTime CreatedAt,
    DateTime UpdatedAt);
