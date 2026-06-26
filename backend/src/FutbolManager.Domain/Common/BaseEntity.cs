namespace FutbolManager.Domain.Common;

/// <summary>
/// Clase base para toda entidad de dominio con identidad y auditoría temporal.
/// </summary>
/// <remarks>
/// <para>
/// Define la <see cref="Id"/> (PK generada por la base de datos) y los campos
/// <see cref="CreatedAt"/> / <see cref="UpdatedAt"/> en UTC.
/// </para>
/// <para>
/// Los setters son <c>protected</c> para que solo las propias entidades puedan
/// mutar su estado (encapsulación). EF Core accede a ellos vía reflexión.
/// </para>
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>Identificador único de la entidad (asignado por la base de datos).</summary>
    public int Id { get; protected set; }

    /// <summary>Fecha y hora UTC en que se creó el registro.</summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>Fecha y hora UTC de la última modificación del registro.</summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Marca la entidad como modificada actualizando <see cref="UpdatedAt"/>
    /// al instante UTC actual.
    /// </summary>
    protected void TouchUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
