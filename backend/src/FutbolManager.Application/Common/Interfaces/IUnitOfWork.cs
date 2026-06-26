namespace FutbolManager.Application.Common.Interfaces;

/// <summary>
/// Abstracción del Unit Of Work. Permite confirmar todos los cambios
/// realizados a través de los repositorios en una única transacción.
/// </summary>
/// <remarks>
/// La implementación concreta vive en la capa Persistence y delega en
/// <c>DbContext.SaveChangesAsync()</c>.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Confirma todos los cambios pendientes contra la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación cooperativa.</param>
    /// <returns>Número de filas afectadas.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
