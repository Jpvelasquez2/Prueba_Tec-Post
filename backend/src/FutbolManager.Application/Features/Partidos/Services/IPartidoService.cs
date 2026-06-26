using FutbolManager.Application.Features.Partidos.Dtos;

namespace FutbolManager.Application.Features.Partidos.Services;

/// <summary>Servicio de aplicación para el CRUD y resultados de partidos.</summary>
public interface IPartidoService
{
    /// <summary>Lista todos los partidos ordenados por fecha desc.</summary>
    Task<IReadOnlyList<PartidoDto>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>Obtiene un partido por id. Lanza <c>NotFoundException</c> si no existe.</summary>
    Task<PartidoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Programa un nuevo partido.</summary>
    Task<PartidoDto> CrearAsync(CrearPartidoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Actualiza un partido existente (datos generales, no su resultado).</summary>
    Task<PartidoDto> ActualizarAsync(int id, ActualizarPartidoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Elimina un partido.</summary>
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra el resultado del partido. Cambia automáticamente el estado a
    /// <see cref="Domain.Enums.EstadoPartido.Jugado"/>.
    /// </summary>
    /// <exception cref="Common.Exceptions.NotFoundException">Si el partido no existe.</exception>
    Task<PartidoDto> RegistrarResultadoAsync(int partidoId, RegistrarResultadoRequest request, CancellationToken cancellationToken = default);
}
