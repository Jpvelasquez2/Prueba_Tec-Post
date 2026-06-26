namespace FutbolManager.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se intenta una operación que viola una regla
/// de negocio del dominio (invariante).
/// </summary>
/// <remarks>
/// La capa Api la traduce a HTTP 400 mediante el middleware global de excepciones.
/// </remarks>
public sealed class BusinessRuleException : Exception
{
    /// <summary>Crea una nueva instancia con el mensaje indicado.</summary>
    /// <param name="message">Mensaje legible que describe la regla violada.</param>
    public BusinessRuleException(string message) : base(message)
    {
    }
}
