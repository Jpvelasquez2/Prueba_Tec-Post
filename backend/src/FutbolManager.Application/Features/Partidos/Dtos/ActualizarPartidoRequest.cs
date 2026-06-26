using System.ComponentModel.DataAnnotations;
using FutbolManager.Domain.Enums;

namespace FutbolManager.Application.Features.Partidos.Dtos;

/// <summary>Payload para actualizar un partido existente (datos generales, no el resultado).</summary>
public sealed class ActualizarPartidoRequest
{
    /// <summary>Nueva fecha.</summary>
    [Required]
    public DateTime Fecha { get; set; }

    /// <summary>Nuevo equipo local.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int LocalTeamId { get; set; }

    /// <summary>Nuevo equipo visitante.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int VisitanteTeamId { get; set; }

    /// <summary>Nuevo estado del partido.</summary>
    [Required]
    public EstadoPartido Estado { get; set; }
}
