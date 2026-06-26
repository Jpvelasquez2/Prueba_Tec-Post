namespace FutbolManager.Application.Features.Equipos.Dtos;

/// <summary>
/// Representación pública de un <see cref="Domain.Entities.Equipo"/>.
/// Es el modelo que devuelve la API.
/// </summary>
/// <param name="Id">Identificador único.</param>
/// <param name="Nombre">Nombre del equipo.</param>
/// <param name="Ciudad">Ciudad (opcional).</param>
/// <param name="EscudoUrl">URL del escudo (opcional).</param>
/// <param name="CreatedAt">Fecha UTC de creación.</param>
/// <param name="UpdatedAt">Fecha UTC de última modificación.</param>
public record EquipoDto(
    int Id,
    string Nombre,
    string? Ciudad,
    string? EscudoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
