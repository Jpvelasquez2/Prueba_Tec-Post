using FutbolManager.Domain.Common;
using FutbolManager.Domain.Exceptions;

namespace FutbolManager.Domain.Entities;

/// <summary>
/// Equipo participante. Agregado raíz simple sin colecciones internas.
/// </summary>
/// <remarks>
/// Entidad rica: las invariantes (nombre no vacío) viven aquí, no en la capa
/// Application. Los setters son privados para forzar el uso de los métodos
/// de mutación (<see cref="Actualizar"/>).
/// </remarks>
public class Equipo : BaseEntity
{
    /// <summary>Nombre único del equipo.</summary>
    public string Nombre { get; private set; } = null!;

    /// <summary>Ciudad de origen (opcional).</summary>
    public string? Ciudad { get; private set; }

    /// <summary>URL del escudo (opcional). Si se informa, debe iniciar con http(s)://.</summary>
    public string? EscudoUrl { get; private set; }

    /// <summary>Constructor sin parámetros requerido por EF Core.</summary>
    private Equipo()
    {
    }

    /// <summary>Crea un nuevo equipo aplicando las invariantes de dominio.</summary>
    /// <param name="nombre">Nombre del equipo. No puede ser nulo ni vacío.</param>
    /// <param name="ciudad">Ciudad de origen (opcional).</param>
    /// <param name="escudoUrl">URL del escudo (opcional).</param>
    /// <exception cref="BusinessRuleException">Si <paramref name="nombre"/> está vacío o <paramref name="escudoUrl"/> no es una URL http(s).</exception>
    public Equipo(string nombre, string? ciudad, string? escudoUrl)
    {
        SetNombre(nombre);
        Ciudad = NormalizarOpcional(ciudad);
        SetEscudoUrl(escudoUrl);
    }

    /// <summary>
    /// Actualiza los datos del equipo. Refresca <see cref="BaseEntity.UpdatedAt"/>.
    /// </summary>
    /// <param name="nombre">Nuevo nombre (obligatorio).</param>
    /// <param name="ciudad">Nueva ciudad (opcional).</param>
    /// <param name="escudoUrl">Nueva URL del escudo (opcional).</param>
    /// <exception cref="BusinessRuleException">Si las invariantes no se cumplen.</exception>
    public void Actualizar(string nombre, string? ciudad, string? escudoUrl)
    {
        SetNombre(nombre);
        Ciudad = NormalizarOpcional(ciudad);
        SetEscudoUrl(escudoUrl);
        TouchUpdatedAt();
    }

    /// <summary>Valida y asigna el nombre tras hacer <c>trim</c>.</summary>
    private void SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new BusinessRuleException("El nombre del equipo no puede estar vacío.");

        Nombre = nombre.Trim();
    }

    /// <summary>Valida el formato y asigna la URL del escudo.</summary>
    private void SetEscudoUrl(string? escudoUrl)
    {
        var valor = NormalizarOpcional(escudoUrl);
        if (valor is not null && !(valor.StartsWith("http://") || valor.StartsWith("https://")))
            throw new BusinessRuleException("EscudoUrl debe iniciar con http:// o https://.");

        EscudoUrl = valor;
    }

    /// <summary>Convierte cadenas vacías o whitespace a null y aplica trim.</summary>
    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
