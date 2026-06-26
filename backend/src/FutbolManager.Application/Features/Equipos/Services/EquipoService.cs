using AutoMapper;
using FutbolManager.Application.Common.Exceptions;
using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Application.Features.Equipos.Dtos;
using FutbolManager.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FutbolManager.Application.Features.Equipos.Services;

/// <summary>
/// Implementación del Service Pattern para equipos.
/// Orquesta el repositorio, el UoW y las invariantes de aplicación.
/// </summary>
public sealed class EquipoService : IEquipoService
{
    private readonly IEquipoRepository _equipoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EquipoService> _logger;

    /// <summary>DI constructor.</summary>
    public EquipoService(
        IEquipoRepository equipoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EquipoService> logger)
    {
        _equipoRepository = equipoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EquipoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var equipos = await _equipoRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<EquipoDto>>(equipos);
    }

    /// <inheritdoc />
    public async Task<EquipoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var equipo = await _equipoRepository.GetByIdAsync(id, cancellationToken)
                     ?? throw new NotFoundException("Equipo", id);

        return _mapper.Map<EquipoDto>(equipo);
    }

    /// <inheritdoc />
    public async Task<EquipoDto> CrearAsync(CrearEquipoRequest request, CancellationToken cancellationToken = default)
    {
        // Regla de aplicación: el nombre debe ser único. Lo verificamos antes de
        // intentar el INSERT para devolver un 409 claro en lugar de un error de
        // restricción única de SQL Server.
        if (await _equipoRepository.ExistsByNombreAsync(request.Nombre, excluirId: null, cancellationToken))
            throw new ConflictException($"Ya existe un equipo con el nombre '{request.Nombre}'.");

        // El constructor de la entidad aplica las invariantes de dominio
        // (nombre no vacío, URL válida).
        var equipo = new Equipo(request.Nombre, request.Ciudad, request.EscudoUrl);

        await _equipoRepository.AddAsync(equipo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Equipo creado: Id={EquipoId}, Nombre={Nombre}", equipo.Id, equipo.Nombre);

        return _mapper.Map<EquipoDto>(equipo);
    }

    /// <inheritdoc />
    public async Task<EquipoDto> ActualizarAsync(int id, ActualizarEquipoRequest request, CancellationToken cancellationToken = default)
    {
        var equipo = await _equipoRepository.GetByIdAsync(id, cancellationToken)
                     ?? throw new NotFoundException("Equipo", id);

        // Validar unicidad de nombre excluyendo al propio registro.
        if (await _equipoRepository.ExistsByNombreAsync(request.Nombre, excluirId: id, cancellationToken))
            throw new ConflictException($"Ya existe otro equipo con el nombre '{request.Nombre}'.");

        // La entidad encapsula la mutación y las invariantes.
        equipo.Actualizar(request.Nombre, request.Ciudad, request.EscudoUrl);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Equipo actualizado: Id={EquipoId}", equipo.Id);

        return _mapper.Map<EquipoDto>(equipo);
    }

    /// <inheritdoc />
    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var equipo = await _equipoRepository.GetByIdAsync(id, cancellationToken)
                     ?? throw new NotFoundException("Equipo", id);

        // No permitimos borrar un equipo que aparezca en partidos: la FK del
        // esquema lo impediría con un error opaco; aquí lo traducimos a 409.
        if (await _equipoRepository.TienePartidosAsync(id, cancellationToken))
            throw new ConflictException("No se puede eliminar el equipo porque tiene partidos asociados.");

        _equipoRepository.Remove(equipo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Equipo eliminado: Id={EquipoId}", id);
    }
}
