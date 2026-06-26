namespace FutbolManager.Domain.Posiciones;

/// <summary>
/// Entrada mínima al calculador: identifica un equipo dentro de la tabla.
/// </summary>
/// <remarks>
/// El calculador solo necesita <see cref="Id"/> (clave para acumular) y
/// <see cref="Nombre"/> (desempate alfabético final). Ciudad, escudo y demás
/// metadatos se enriquecen fuera del calculador.
/// </remarks>
/// <param name="Id">Identificador único del equipo.</param>
/// <param name="Nombre">Nombre del equipo (usado como tiebreaker determinístico).</param>
public sealed record EquipoParticipante(int Id, string Nombre);
