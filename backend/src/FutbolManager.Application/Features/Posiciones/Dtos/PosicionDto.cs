namespace FutbolManager.Application.Features.Posiciones.Dtos;

/// <summary>
/// Una fila de la tabla de posiciones (un equipo con sus acumulados).
/// </summary>
/// <param name="Posicion">Lugar en la tabla, partiendo de 1.</param>
/// <param name="EquipoId">Id del equipo.</param>
/// <param name="Equipo">Nombre del equipo.</param>
/// <param name="Ciudad">Ciudad del equipo (opcional).</param>
/// <param name="EscudoUrl">URL del escudo (opcional).</param>
/// <param name="PJ">Partidos Jugados.</param>
/// <param name="PG">Partidos Ganados.</param>
/// <param name="PE">Partidos Empatados.</param>
/// <param name="PP">Partidos Perdidos.</param>
/// <param name="GF">Goles a Favor.</param>
/// <param name="GC">Goles en Contra.</param>
/// <param name="DG">Diferencia de Goles (GF - GC).</param>
/// <param name="PTS">Puntos (V=3, E=1, D=0).</param>
public record PosicionDto(
    int Posicion,
    int EquipoId,
    string Equipo,
    string? Ciudad,
    string? EscudoUrl,
    int PJ,
    int PG,
    int PE,
    int PP,
    int GF,
    int GC,
    int DG,
    int PTS);
