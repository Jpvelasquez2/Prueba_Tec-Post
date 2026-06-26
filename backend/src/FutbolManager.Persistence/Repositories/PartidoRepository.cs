using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Domain.Entities;
using FutbolManager.Domain.Enums;
using FutbolManager.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FutbolManager.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de partidos.
/// </summary>
public sealed class PartidoRepository : IPartidoRepository
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>DI constructor.</summary>
    public PartidoRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Partido>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Carga ansiosa de los equipos: el DTO los necesita para mostrar nombres.
        return await _dbContext.Partidos
            .AsNoTracking()
            .Include(p => p.LocalTeam)
            .Include(p => p.VisitanteTeam)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Partido?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Partidos
            .Include(p => p.LocalTeam)
            .Include(p => p.VisitanteTeam)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Partido>> GetJugadosAsync(CancellationToken cancellationToken = default)
    {
        // No necesitamos los equipos cargados: el cálculo se hace solo por ids.
        return await _dbContext.Partidos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoPartido.Jugado)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Partido partido, CancellationToken cancellationToken = default)
    {
        await _dbContext.Partidos.AddAsync(partido, cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(Partido partido) => _dbContext.Partidos.Remove(partido);
}
