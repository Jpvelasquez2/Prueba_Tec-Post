using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Persistence.Contexts;

namespace FutbolManager.Persistence.UnitOfWork;

/// <summary>
/// Implementación del Unit Of Work sobre EF Core.
/// Confirma todos los cambios pendientes del <see cref="ApplicationDbContext"/>.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>DI constructor.</summary>
    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
