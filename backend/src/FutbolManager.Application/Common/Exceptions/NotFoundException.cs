namespace FutbolManager.Application.Common.Exceptions;

/// <summary>
/// Excepción de aplicación lanzada cuando un recurso solicitado no existe.
/// La capa Api la traduce a HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Crea una excepción con mensaje libre.</summary>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Crea una excepción con mensaje estandarizado para una entidad e id.
    /// </summary>
    /// <param name="entidad">Nombre legible de la entidad (p. ej. "Equipo").</param>
    /// <param name="id">Identificador buscado.</param>
    public NotFoundException(string entidad, object id)
        : base($"No se encontró {entidad} con id '{id}'.")
    {
    }
}
