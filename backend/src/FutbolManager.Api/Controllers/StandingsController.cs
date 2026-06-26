using FutbolManager.Application.Features.Posiciones.Dtos;
using FutbolManager.Application.Features.Posiciones.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutbolManager.Api.Controllers;

/// <summary>
/// Endpoint de la tabla de posiciones.
/// </summary>
[ApiController]
[Route("api/standings")]
[Produces("application/json")]
public sealed class StandingsController : ControllerBase
{
    private readonly IPosicionesService _posicionesService;

    /// <summary>DI constructor.</summary>
    public StandingsController(IPosicionesService posicionesService)
    {
        _posicionesService = posicionesService;
    }

    /// <summary>
    /// Calcula y devuelve la tabla de posiciones a partir de los partidos jugados.
    /// </summary>
    /// <remarks>
    /// Orden: PTS desc, DG desc, GF desc, Nombre asc.
    /// El criterio head-to-head no se aplica (ver scripts/db/README.md).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PosicionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PosicionDto>>> Obtener(CancellationToken cancellationToken)
    {
        var tabla = await _posicionesService.CalcularAsync(cancellationToken);
        return Ok(tabla);
    }
}
