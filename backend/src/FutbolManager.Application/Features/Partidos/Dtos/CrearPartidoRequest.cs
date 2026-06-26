using System.ComponentModel.DataAnnotations;
using FutbolManager.Domain.Enums;

namespace FutbolManager.Application.Features.Partidos.Dtos;

/// <summary>Payload para programar un partido.</summary>
public sealed class CrearPartidoRequest
{
    /// <summary>Fecha y hora del partido (UTC).</summary>
    [Required]
    public DateTime Fecha { get; set; }

    /// <summary>Id del equipo local.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int LocalTeamId { get; set; }

    /// <summary>Id del equipo visitante.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int VisitanteTeamId { get; set; }

    /// <summary>
    /// Estado inicial. Si no se envía, el servicio asume <see cref="EstadoPartido.Programado"/>.
    /// </summary>
    public EstadoPartido? Estado { get; set; }
}
