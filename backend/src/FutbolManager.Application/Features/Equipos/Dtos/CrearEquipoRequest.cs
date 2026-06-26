using System.ComponentModel.DataAnnotations;

namespace FutbolManager.Application.Features.Equipos.Dtos;

/// <summary>Payload para crear un equipo.</summary>
public sealed class CrearEquipoRequest
{
    /// <summary>Nombre del equipo (obligatorio, 1-100 caracteres).</summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Ciudad (opcional, hasta 100 caracteres).</summary>
    [StringLength(100)]
    public string? Ciudad { get; set; }

    /// <summary>URL del escudo (opcional, hasta 500 caracteres).</summary>
    [StringLength(500)]
    [Url]
    public string? EscudoUrl { get; set; }
}
