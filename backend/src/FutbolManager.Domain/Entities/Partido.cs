using FutbolManager.Domain.Common;
using FutbolManager.Domain.Enums;
using FutbolManager.Domain.Exceptions;

namespace FutbolManager.Domain.Entities;

/// <summary>
/// Partido entre dos equipos. Encapsula las reglas de:
/// equipos distintos, marcadores no negativos, transición de estados y
/// registro de resultado.
/// </summary>
public class Partido : BaseEntity
{
    /// <summary>Fecha y hora programada (o jugada) del partido.</summary>
    public DateTime Fecha { get; private set; }

    /// <summary>Identificador del equipo local.</summary>
    public int LocalTeamId { get; private set; }

    /// <summary>Identificador del equipo visitante.</summary>
    public int VisitanteTeamId { get; private set; }

    /// <summary>Goles del equipo local. <c>null</c> hasta que el partido se juega.</summary>
    public byte? LocalScore { get; private set; }

    /// <summary>Goles del equipo visitante. <c>null</c> hasta que el partido se juega.</summary>
    public byte? VisitanteScore { get; private set; }

    /// <summary>Estado actual del partido.</summary>
    public EstadoPartido Estado { get; private set; }

    /// <summary>Navegación al equipo local (carga opcional desde Persistence).</summary>
    public Equipo? LocalTeam { get; private set; }

    /// <summary>Navegación al equipo visitante (carga opcional desde Persistence).</summary>
    public Equipo? VisitanteTeam { get; private set; }

    /// <summary>Constructor sin parámetros requerido por EF Core.</summary>
    private Partido()
    {
    }

    /// <summary>
    /// Crea un nuevo partido, por defecto en estado <see cref="EstadoPartido.Programado"/>.
    /// </summary>
    /// <param name="fecha">Fecha y hora del partido.</param>
    /// <param name="localTeamId">Id del equipo local.</param>
    /// <param name="visitanteTeamId">Id del equipo visitante.</param>
    /// <param name="estado">Estado inicial (por defecto Programado).</param>
    /// <exception cref="BusinessRuleException">Si los equipos coinciden.</exception>
    public Partido(DateTime fecha, int localTeamId, int visitanteTeamId,
                   EstadoPartido estado = EstadoPartido.Programado)
    {
        ValidarEquiposDistintos(localTeamId, visitanteTeamId);

        Fecha = fecha;
        LocalTeamId = localTeamId;
        VisitanteTeamId = visitanteTeamId;
        Estado = estado;
    }

    /// <summary>
    /// Actualiza los datos generales de un partido (no su resultado).
    /// </summary>
    /// <remarks>
    /// Si el nuevo estado no es <see cref="EstadoPartido.Jugado"/>, los marcadores
    /// se limpian para mantener la coherencia con el constraint
    /// <c>CK_Partidos_JugadoTieneResultado</c>.
    /// </remarks>
    /// <exception cref="BusinessRuleException">
    /// Si el partido está cancelado o si los equipos coinciden.
    /// </exception>
    public void Actualizar(DateTime fecha, int localTeamId, int visitanteTeamId, EstadoPartido estado)
    {
        if (Estado == EstadoPartido.Cancelado)
            throw new BusinessRuleException("No se puede editar un partido cancelado.");

        ValidarEquiposDistintos(localTeamId, visitanteTeamId);

        Fecha = fecha;
        LocalTeamId = localTeamId;
        VisitanteTeamId = visitanteTeamId;
        Estado = estado;

        // Si el nuevo estado no es Jugado, el resultado deja de tener sentido.
        if (estado != EstadoPartido.Jugado)
        {
            LocalScore = null;
            VisitanteScore = null;
        }

        TouchUpdatedAt();
    }

    /// <summary>
    /// Registra el resultado del partido y lo deja en estado
    /// <see cref="EstadoPartido.Jugado"/>.
    /// </summary>
    /// <param name="localScore">Goles del equipo local (≥ 0).</param>
    /// <param name="visitanteScore">Goles del equipo visitante (≥ 0).</param>
    /// <exception cref="BusinessRuleException">
    /// Si el partido está cancelado, los goles son negativos o exceden <see cref="byte.MaxValue"/>.
    /// </exception>
    public void RegistrarResultado(int localScore, int visitanteScore)
    {
        if (Estado == EstadoPartido.Cancelado)
            throw new BusinessRuleException("No se puede registrar resultado en un partido cancelado.");

        if (localScore < 0 || visitanteScore < 0)
            throw new BusinessRuleException("Los goles no pueden ser negativos.");

        if (localScore > byte.MaxValue || visitanteScore > byte.MaxValue)
            throw new BusinessRuleException($"Los goles no pueden superar {byte.MaxValue}.");

        LocalScore = (byte)localScore;
        VisitanteScore = (byte)visitanteScore;
        Estado = EstadoPartido.Jugado;  // Transición automática a Jugado.
        TouchUpdatedAt();
    }

    /// <summary>
    /// Cambia el estado del partido sin tocar el resultado.
    /// Útil para suspender o cancelar sin pasar por <see cref="Actualizar"/>.
    /// </summary>
    /// <exception cref="BusinessRuleException">
    /// Si el partido ya está cancelado y se intenta volver a Programado/Jugado.
    /// </exception>
    public void CambiarEstado(EstadoPartido nuevoEstado)
    {
        if (Estado == EstadoPartido.Cancelado && nuevoEstado != EstadoPartido.Cancelado)
            throw new BusinessRuleException("Un partido cancelado no puede regresar a otro estado.");

        Estado = nuevoEstado;

        // Si pierde el estado Jugado, los marcadores ya no tienen sentido.
        if (nuevoEstado != EstadoPartido.Jugado)
        {
            LocalScore = null;
            VisitanteScore = null;
        }

        TouchUpdatedAt();
    }

    /// <summary>
    /// Invariante: un equipo no puede jugar contra sí mismo.
    /// </summary>
    /// <exception cref="BusinessRuleException">Si <paramref name="local"/> y <paramref name="visitante"/> coinciden.</exception>
    private static void ValidarEquiposDistintos(int local, int visitante)
    {
        if (local == visitante)
            throw new BusinessRuleException("Un equipo no puede jugar contra sí mismo.");
    }
}
