using FutbolManager.Domain.Entities;

namespace FutbolManager.Application.Common.Interfaces;

/// <summary>
/// Puerto de acceso a datos para la entidad <see cref="Equipo"/>.
/// Implementado por la capa Persistence (Repository Pattern).
/// </summary>
public interface IEquipoRepository
{
    /// <summary>Obtiene todos los equipos ordenados por nombre.</summary>
    Task<IReadOnlyList<Equipo>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un equipo por su id, o <c>null</c> si no existe.</summary>
    Task<Equipo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Indica si existe algún equipo con el nombre indicado (case-insensitive).</summary>
    /// <param name="nombre">Nombre exacto a verificar.</param>
    /// <param name="excluirId">Id que se debe excluir del chequeo (útil al actualizar).</param>
    /// <param name="cancellationToken">Token de cancelación cooperativa.</param>
    Task<bool> ExistsByNombreAsync(string nombre, int? excluirId = null, CancellationToken cancellationToken = default);

    /// <summary>Indica si existe un equipo con el id indicado.</summary>
    Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Indica si el equipo tiene partidos asociados (local o visitante).</summary>
    Task<bool> TienePartidosAsync(int equipoId, CancellationToken cancellationToken = default);

    /// <summary>Agrega un nuevo equipo al contexto. No persiste hasta llamar al UoW.</summary>
    Task AddAsync(Equipo equipo, CancellationToken cancellationToken = default);

    /// <summary>Marca el equipo como eliminado en el contexto.</summary>
    void Remove(Equipo equipo);
}
