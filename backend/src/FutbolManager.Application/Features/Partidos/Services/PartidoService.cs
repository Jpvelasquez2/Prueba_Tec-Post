using AutoMapper;
using FutbolManager.Application.Common.Exceptions;
using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Application.Features.Partidos.Dtos;
using FutbolManager.Domain.Entities;
using FutbolManager.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FutbolManager.Application.Features.Partidos.Services;

/// <summary>Implementación del Service Pattern para partidos.</summary>
public sealed class PartidoService : IPartidoService
{
    private readonly IPartidoRepository _partidoRepository;
    private readonly IEquipoRepository _equipoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PartidoService> _logger;

    /// <summary>DI constructor.</summary>
    public PartidoService(
        IPartidoRepository partidoRepository,
        IEquipoRepository equipoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PartidoService> logger)
    {
        _partidoRepository = partidoRepository;
        _equipoRepository = equipoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PartidoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var partidos = await _partidoRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<PartidoDto>>(partidos);
    }

    /// <inheritdoc />
    public async Task<PartidoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var partido = await _partidoRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Partido", id);

        return _mapper.Map<PartidoDto>(partido);
    }

    /// <inheritdoc />
    public async Task<PartidoDto> CrearAsync(CrearPartidoRequest request, CancellationToken cancellationToken = default)
    {
        // Validamos que los equipos existan antes de delegar al dominio
        // (el constructor solo se ocupa de la invariante "equipos distintos").
        await GarantizarEquiposExistenAsync(request.LocalTeamId, request.VisitanteTeamId, cancellationToken);

        var estadoInicial = request.Estado ?? EstadoPartido.Programado;
        var partido = new Partido(request.Fecha, request.LocalTeamId, request.VisitanteTeamId, estadoInicial);

        await _partidoRepository.AddAsync(partido, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Partido programado: Id={PartidoId}, Local={Local}, Visitante={Visitante}, Fecha={Fecha:o}",
            partido.Id, partido.LocalTeamId, partido.VisitanteTeamId, partido.Fecha);

        // Recargar para que la respuesta incluya los nombres de los equipos.
        return await ObtenerPorIdAsync(partido.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PartidoDto> ActualizarAsync(int id, ActualizarPartidoRequest request, CancellationToken cancellationToken = default)
    {
        var partido = await _partidoRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Partido", id);

        await GarantizarEquiposExistenAsync(request.LocalTeamId, request.VisitanteTeamId, cancellationToken);

        // El dominio aplica las reglas: no se puede editar si está cancelado, etc.
        partido.Actualizar(request.Fecha, request.LocalTeamId, request.VisitanteTeamId, request.Estado);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Partido actualizado: Id={PartidoId}", id);

        return await ObtenerPorIdAsync(partido.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var partido = await _partidoRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Partido", id);

        _partidoRepository.Remove(partido);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Partido eliminado: Id={PartidoId}", id);
    }

    /// <inheritdoc />
    public async Task<PartidoDto> RegistrarResultadoAsync(int partidoId, RegistrarResultadoRequest request, CancellationToken cancellationToken = default)
    {
        // Validación requerida: no permitir registrar si no existe.
        var partido = await _partidoRepository.GetByIdAsync(partidoId, cancellationToken)
                      ?? throw new NotFoundException("Partido", partidoId);

        // El dominio aplica todas las reglas del registro: cancelado, negativos,
        // sobrepasar 255, y transiciona automáticamente a Estado=Jugado.
        partido.RegistrarResultado(request.LocalScore, request.VisitanteScore);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Resultado registrado: PartidoId={PartidoId}, {Local}-{Visitante}",
            partidoId, request.LocalScore, request.VisitanteScore);

        return await ObtenerPorIdAsync(partidoId, cancellationToken);
    }

    /// <summary>
    /// Valida que ambos equipos existan; lanza <see cref="NotFoundException"/>
    /// si alguno no se encuentra.
    /// </summary>
    private async Task GarantizarEquiposExistenAsync(int localTeamId, int visitanteTeamId, CancellationToken cancellationToken)
    {
        if (!await _equipoRepository.ExistsByIdAsync(localTeamId, cancellationToken))
            throw new NotFoundException("Equipo local", localTeamId);

        if (!await _equipoRepository.ExistsByIdAsync(visitanteTeamId, cancellationToken))
            throw new NotFoundException("Equipo visitante", visitanteTeamId);
    }
}
