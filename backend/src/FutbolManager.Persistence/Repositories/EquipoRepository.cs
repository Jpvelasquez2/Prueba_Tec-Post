using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Domain.Entities;
using FutbolManager.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FutbolManager.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de equipos.
/// </summary>
public sealed class EquipoRepository : IEquipoRepository
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>DI constructor.</summary>
    public EquipoRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Equipo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking porque es solo lectura: evita cargar el tracker y mejora performance.
        return await _dbContext.Equipos
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Equipo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Aquí SÍ trackeamos: el caller normalmente quiere modificar el agregado.
        return await _dbContext.Equipos
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNombreAsync(string nombre, int? excluirId = null, CancellationToken cancellationToken = default)
    {
        // Comparación case-insensitive: el collation Modern_Spanish_CI_AS de la BD
        // hace que la comparación a nivel SQL sea CI por defecto.
        return await _dbContext.Equipos
            .AsNoTracking()
            .AnyAsync(e => e.Nombre == nombre && (excluirId == null || e.Id != excluirId.Value), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbContext.Equipos.AsNoTracking().AnyAsync(e => e.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> TienePartidosAsync(int equipoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Partidos
            .AsNoTracking()
            .AnyAsync(p => p.LocalTeamId == equipoId || p.VisitanteTeamId == equipoId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Equipo equipo, CancellationToken cancellationToken = default)
    {
        await _dbContext.Equipos.AddAsync(equipo, cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(Equipo equipo) => _dbContext.Equipos.Remove(equipo);
}
