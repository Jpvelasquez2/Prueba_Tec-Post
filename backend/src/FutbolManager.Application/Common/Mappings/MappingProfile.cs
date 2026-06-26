using AutoMapper;
using FutbolManager.Application.Features.Equipos.Dtos;
using FutbolManager.Application.Features.Partidos.Dtos;
using FutbolManager.Domain.Entities;

namespace FutbolManager.Application.Common.Mappings;

/// <summary>
/// Perfil de AutoMapper. Define las conversiones entidad ↔ DTO.
/// </summary>
/// <remarks>
/// Mantenemos un único perfil porque hay pocas entidades. Si crece la solución,
/// conviene dividirlo en uno por feature (EquipoMappingProfile, etc.).
/// </remarks>
public sealed class MappingProfile : Profile
{
    /// <summary>Configura los mapeos.</summary>
    public MappingProfile()
    {
        // -----------------------------------------------------------------
        // Equipo → EquipoDto: mapeo directo, todos los nombres coinciden.
        // -----------------------------------------------------------------
        CreateMap<Equipo, EquipoDto>();

        // -----------------------------------------------------------------
        // Partido → PartidoDto: requiere proyectar los nombres de los equipos
        // desde las navegaciones (cargadas por el repositorio con Include).
        // -----------------------------------------------------------------
        CreateMap<Partido, PartidoDto>()
            .ForCtorParam(nameof(PartidoDto.LocalTeamNombre),
                          opt => opt.MapFrom(src => src.LocalTeam != null ? src.LocalTeam.Nombre : string.Empty))
            .ForCtorParam(nameof(PartidoDto.VisitanteTeamNombre),
                          opt => opt.MapFrom(src => src.VisitanteTeam != null ? src.VisitanteTeam.Nombre : string.Empty));
    }
}
