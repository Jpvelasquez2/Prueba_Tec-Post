using System.ComponentModel.DataAnnotations;

namespace FutbolManager.Application.Features.Partidos.Dtos;

/// <summary>
/// Payload para registrar el resultado de un partido.
/// El servicio cambia automáticamente el estado a Jugado al persistir.
/// </summary>
public sealed class RegistrarResultadoRequest
{
    /// <summary>Goles del equipo local (≥ 0, ≤ 255).</summary>
    [Required]
    [Range(0, byte.MaxValue)]
    public int LocalScore { get; set; }

    /// <summary>Goles del equipo visitante (≥ 0, ≤ 255).</summary>
    [Required]
    [Range(0, byte.MaxValue)]
    public int VisitanteScore { get; set; }
}
