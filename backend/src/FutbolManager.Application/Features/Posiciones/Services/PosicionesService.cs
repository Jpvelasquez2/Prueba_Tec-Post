using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Application.Features.Posiciones.Dtos;
using FutbolManager.Domain.Posiciones;
using Microsoft.Extensions.Logging;

namespace FutbolManager.Application.Features.Posiciones.Services;

/// <summary>
/// Orquestador de la tabla de posiciones.
/// </summary>
/// <remarks>
/// <para>
/// Esta clase ya <b>no</b> contiene el cálculo: delega íntegramente en
/// <see cref="CalculadorTablaPosiciones"/> (Domain). Su única responsabilidad
/// es traer los datos (repositorios), invocar al calculador y enriquecer
/// la salida con metadatos del equipo (ciudad, escudo) que el calculador no
/// conoce.
/// </para>
/// <para>
/// Esta separación cumple con el Principio de Responsabilidad Única (SRP):
/// los cambios al algoritmo se hacen en Domain; los cambios en cómo se
/// cargan los datos o se exponen, aquí.
/// </para>
/// </remarks>
public sealed class PosicionesService : IPosicionesService
{
    private readonly IEquipoRepository _equipoRepository;
    private readonly IPartidoRepository _partidoRepository;
    private readonly CalculadorTablaPosiciones _calculador;
    private readonly ILogger<PosicionesService> _logger;

    /// <summary>DI constructor.</summary>
    public PosicionesService(
        IEquipoRepository equipoRepository,
        IPartidoRepository partidoRepository,
        CalculadorTablaPosiciones calculador,
        ILogger<PosicionesService> logger)
    {
        _equipoRepository = equipoRepository;
        _partidoRepository = partidoRepository;
        _calculador = calculador;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PosicionDto>> CalcularAsync(CancellationToken cancellationToken = default)
    {
        // 1) Traer equipos y partidos jugados.
        var equipos = await _equipoRepository.GetAllAsync(cancellationToken);
        var partidosJugados = await _partidoRepository.GetJugadosAsync(cancellationToken);

        // 2) Proyectar a las entradas que entiende el calculador (sin EF, sin enums).
        var entradaEquipos = equipos
            .Select(e => new EquipoParticipante(e.Id, e.Nombre))
            .ToList();

        var entradaResultados = partidosJugados
            // Defensivo: descarta partidos sin marcador (no deberían existir si
            // el estado es Jugado, pero el calculador exige valores concretos).
            .Where(p => p.LocalScore.HasValue && p.VisitanteScore.HasValue)
            .Select(p => new ResultadoPartido(
                EquipoLocalId:     p.LocalTeamId,
                EquipoVisitanteId: p.VisitanteTeamId,
                GolesLocal:        p.LocalScore!.Value,
                GolesVisitante:    p.VisitanteScore!.Value))
            .ToList();

        // 3) Delegar el cálculo al algoritmo puro.
        var tabla = _calculador.Calcular(entradaEquipos, entradaResultados);

        // 4) Enriquecer la salida con ciudad y escudo (que el calculador desconoce).
        var indiceEquipos = equipos.ToDictionary(e => e.Id);
        var resultado = tabla
            .Select(fila =>
            {
                var equipo = indiceEquipos[fila.EquipoId];
                return new PosicionDto(
                    Posicion:  fila.Posicion,
                    EquipoId:  fila.EquipoId,
                    Equipo:    fila.Equipo,
                    Ciudad:    equipo.Ciudad,
                    EscudoUrl: equipo.EscudoUrl,
                    PJ:        fila.Pj,
                    PG:        fila.Pg,
                    PE:        fila.Pe,
                    PP:        fila.Pp,
                    GF:        fila.Gf,
                    GC:        fila.Gc,
                    DG:        fila.Dg,
                    PTS:       fila.Pts);
            })
            .ToList();

        _logger.LogInformation(
            "Tabla de posiciones calculada: {Equipos} equipos, {Partidos} partidos contabilizados.",
            equipos.Count, entradaResultados.Count);

        return resultado;
    }
}
