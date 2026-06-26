using System.ComponentModel.DataAnnotations;

namespace FutbolManager.Application.Features.Equipos.Dtos;

/// <summary>Payload para actualizar un equipo existente.</summary>
public sealed class ActualizarEquipoRequest
{
    /// <summary>Nuevo nombre (obligatorio, 1-100 caracteres).</summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Nueva ciudad (opcional).</summary>
    [StringLength(100)]
    public string? Ciudad { get; set; }

    /// <summary>Nueva URL del escudo (opcional).</summary>
    [StringLength(500)]
    [Url]
    public string? EscudoUrl { get; set; }
}
