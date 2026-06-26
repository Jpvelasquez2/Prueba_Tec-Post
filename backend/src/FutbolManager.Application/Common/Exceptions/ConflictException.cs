namespace FutbolManager.Application.Common.Exceptions;

/// <summary>
/// Excepción de aplicación lanzada cuando una operación entra en conflicto
/// con el estado actual de los datos (p. ej. nombre duplicado, eliminación
/// con dependencias). La capa Api la traduce a HTTP 409.
/// </summary>
public sealed class ConflictException : Exception
{
    /// <summary>Crea una excepción de conflicto con mensaje libre.</summary>
    public ConflictException(string message) : base(message)
    {
    }
}
