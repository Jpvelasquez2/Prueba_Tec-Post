using FutbolManager.Application.Features.Equipos.Dtos;
using FutbolManager.Application.Features.Equipos.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutbolManager.Api.Controllers;

/// <summary>
/// Endpoints CRUD para equipos.
/// </summary>
[ApiController]
[Route("api/teams")]
[Produces("application/json")]
public sealed class TeamsController : ControllerBase
{
    private readonly IEquipoService _equipoService;

    /// <summary>DI constructor.</summary>
    public TeamsController(IEquipoService equipoService)
    {
        _equipoService = equipoService;
    }

    /// <summary>Lista todos los equipos.</summary>
    /// <returns>Colección de equipos ordenada por nombre.</returns>
    /// <response code="200">Listado retornado correctamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EquipoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EquipoDto>>> Listar(CancellationToken cancellationToken)
    {
        var equipos = await _equipoService.ListarAsync(cancellationToken);
        return Ok(equipos);
    }

    /// <summary>Obtiene un equipo por id.</summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <response code="200">Equipo encontrado.</response>
    /// <response code="404">No existe equipo con ese id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipoDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var equipo = await _equipoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(equipo);
    }

    /// <summary>Crea un nuevo equipo.</summary>
    /// <param name="request">Datos del nuevo equipo.</param>
    /// <response code="201">Equipo creado. Cabecera <c>Location</c> apunta al recurso.</response>
    /// <response code="400">Payload inválido o regla de dominio violada.</response>
    /// <response code="409">Ya existe un equipo con el mismo nombre.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipoDto>> Crear([FromBody] CrearEquipoRequest request, CancellationToken cancellationToken)
    {
        var equipo = await _equipoService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obtener), new { id = equipo.Id }, equipo);
    }

    /// <summary>Actualiza un equipo existente.</summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="request">Datos actualizados.</param>
    /// <response code="200">Equipo actualizado.</response>
    /// <response code="404">No existe equipo con ese id.</response>
    /// <response code="409">El nuevo nombre choca con otro equipo.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EquipoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipoDto>> Actualizar(int id, [FromBody] ActualizarEquipoRequest request, CancellationToken cancellationToken)
    {
        var equipo = await _equipoService.ActualizarAsync(id, request, cancellationToken);
        return Ok(equipo);
    }

    /// <summary>Elimina un equipo.</summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <response code="204">Equipo eliminado.</response>
    /// <response code="404">No existe equipo con ese id.</response>
    /// <response code="409">El equipo tiene partidos asociados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _equipoService.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
