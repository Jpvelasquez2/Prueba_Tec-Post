using FutbolManager.Application.Features.Partidos.Dtos;
using FutbolManager.Application.Features.Partidos.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutbolManager.Api.Controllers;

/// <summary>
/// Endpoints CRUD para partidos y registro de resultados.
/// </summary>
[ApiController]
[Route("api/matches")]
[Produces("application/json")]
public sealed class MatchesController : ControllerBase
{
    private readonly IPartidoService _partidoService;

    /// <summary>DI constructor.</summary>
    public MatchesController(IPartidoService partidoService)
    {
        _partidoService = partidoService;
    }

    /// <summary>Lista todos los partidos (más recientes primero).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PartidoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PartidoDto>>> Listar(CancellationToken cancellationToken)
    {
        var partidos = await _partidoService.ListarAsync(cancellationToken);
        return Ok(partidos);
    }

    /// <summary>Obtiene un partido por id.</summary>
    /// <response code="200">Partido encontrado.</response>
    /// <response code="404">No existe partido con ese id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PartidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartidoDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var partido = await _partidoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(partido);
    }

    /// <summary>Programa un nuevo partido.</summary>
    /// <response code="201">Partido creado.</response>
    /// <response code="400">Equipos iguales o payload inválido.</response>
    /// <response code="404">Alguno de los equipos no existe.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PartidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartidoDto>> Crear([FromBody] CrearPartidoRequest request, CancellationToken cancellationToken)
    {
        var partido = await _partidoService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = partido.Id }, partido);
    }

    /// <summary>Actualiza un partido existente (datos generales, no su resultado).</summary>
    /// <response code="200">Partido actualizado.</response>
    /// <response code="400">No se puede editar un partido cancelado o equipos iguales.</response>
    /// <response code="404">Partido o algún equipo no existe.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PartidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartidoDto>> Actualizar(int id, [FromBody] ActualizarPartidoRequest request, CancellationToken cancellationToken)
    {
        var partido = await _partidoService.ActualizarAsync(id, request, cancellationToken);
        return Ok(partido);
    }

    /// <summary>Elimina un partido.</summary>
    /// <response code="204">Partido eliminado.</response>
    /// <response code="404">No existe partido con ese id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _partidoService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Registra el resultado de un partido. Cambia el estado a Jugado automáticamente.
    /// </summary>
    /// <param name="id">Id del partido.</param>
    /// <param name="request">Goles de local y visitante (≥ 0).</param>
    /// <response code="200">Resultado registrado.</response>
    /// <response code="400">Partido cancelado o goles inválidos.</response>
    /// <response code="404">No existe partido con ese id.</response>
    [HttpPost("{id:int}/result")]
    [ProducesResponseType(typeof(PartidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartidoDto>> RegistrarResultado(int id, [FromBody] RegistrarResultadoRequest request, CancellationToken cancellationToken)
    {
        var partido = await _partidoService.RegistrarResultadoAsync(id, request, cancellationToken);
        return Ok(partido);
    }
}
