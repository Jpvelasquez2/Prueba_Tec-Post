namespace FutbolManager.Domain.Posiciones;

/// <summary>
/// Salida del calculador: una fila de la tabla de posiciones con todos los
/// agregados ya calculados y la posición asignada.
/// </summary>
/// <param name="Posicion">Lugar en la tabla, partiendo de 1.</param>
/// <param name="EquipoId">Id del equipo.</param>
/// <param name="Equipo">Nombre del equipo.</param>
/// <param name="Pj">Partidos Jugados.</param>
/// <param name="Pg">Partidos Ganados.</param>
/// <param name="Pe">Partidos Empatados.</param>
/// <param name="Pp">Partidos Perdidos.</param>
/// <param name="Gf">Goles a Favor.</param>
/// <param name="Gc">Goles en Contra.</param>
/// <param name="Dg">Diferencia de Goles (GF − GC).</param>
/// <param name="Pts">Puntos (PG × 3 + PE × 1).</param>
public sealed record FilaPosicion(
    int Posicion,
    int EquipoId,
    string Equipo,
    int Pj,
    int Pg,
    int Pe,
    int Pp,
    int Gf,
    int Gc,
    int Dg,
    int Pts);
