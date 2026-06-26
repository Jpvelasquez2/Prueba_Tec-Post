using FluentAssertions;
using FutbolManager.Domain.Entities;
using FutbolManager.Domain.Enums;
using FutbolManager.Domain.Exceptions;
using Xunit;

namespace FutbolManager.Domain.UnitTests.Entities;

/// <summary>
/// Pruebas sobre las reglas de negocio encapsuladas en la entidad rica <see cref="Partido"/>.
/// Como es Domain puro, tampoco se usa Moq.
/// </summary>
public class PartidoTests
{
    // -------------------------------------------------------------------------
    // GRUPO 1 — Construcción
    // -------------------------------------------------------------------------

    [Fact]
    public void Crear_partido_con_equipos_iguales_lanza_BusinessRuleException()
    {
        // Arrange: id local y visitante coincidentes.
        var fecha = DateTime.UtcNow;

        // Act
        var act = () => new Partido(fecha, localTeamId: 1, visitanteTeamId: 1);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*no puede jugar contra sí mismo*");
    }

    [Fact]
    public void Crear_partido_por_defecto_queda_en_estado_Programado()
    {
        // Arrange + Act
        var partido = new Partido(DateTime.UtcNow, 1, 2);

        // Assert
        partido.Estado.Should().Be(EstadoPartido.Programado);
        partido.LocalScore.Should().BeNull();
        partido.VisitanteScore.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // GRUPO 2 — Registrar resultado (regla central solicitada)
    // -------------------------------------------------------------------------

    [Fact]
    public void RegistrarResultado_cambia_el_estado_a_Jugado_automaticamente()
    {
        // Arrange: partido programado.
        var partido = new Partido(DateTime.UtcNow, 1, 2);

        // Act
        partido.RegistrarResultado(localScore: 2, visitanteScore: 1);

        // Assert: el método transiciona automáticamente a Jugado.
        partido.Estado.Should().Be(EstadoPartido.Jugado);
    }

    [Fact]
    public void RegistrarResultado_actualiza_marcadores_y_UpdatedAt()
    {
        // Arrange: partido programado en el pasado.
        var partido = new Partido(DateTime.UtcNow.AddHours(-1), 1, 2);
        var antesDeRegistrar = partido.UpdatedAt;

        // Act
        partido.RegistrarResultado(3, 0);

        // Assert
        partido.LocalScore.Should().Be(3);
        partido.VisitanteScore.Should().Be(0);
        partido.UpdatedAt.Should().BeOnOrAfter(antesDeRegistrar);
    }

    [Fact]
    public void RegistrarResultado_en_partido_cancelado_lanza_BusinessRuleException()
    {
        // Arrange: partido que se cancela.
        var partido = new Partido(DateTime.UtcNow, 1, 2);
        partido.CambiarEstado(EstadoPartido.Cancelado);

        // Act
        var act = () => partido.RegistrarResultado(1, 0);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*cancelado*");
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(-3, -2)]
    public void RegistrarResultado_con_goles_negativos_lanza_BusinessRuleException(int local, int visitante)
    {
        // Arrange
        var partido = new Partido(DateTime.UtcNow, 1, 2);

        // Act
        var act = () => partido.RegistrarResultado(local, visitante);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*no pueden ser negativos*");
    }

    [Fact]
    public void RegistrarResultado_con_goles_que_exceden_byte_lanza_BusinessRuleException()
    {
        // Arrange: marcador absurdo para forzar el límite (>255).
        var partido = new Partido(DateTime.UtcNow, 1, 2);

        // Act
        var act = () => partido.RegistrarResultado(localScore: 256, visitanteScore: 0);

        // Assert: el dominio rechaza marcadores fuera del rango TINYINT.
        act.Should().Throw<BusinessRuleException>();
    }

    // -------------------------------------------------------------------------
    // GRUPO 3 — Actualizar / cambiar estado
    // -------------------------------------------------------------------------

    [Fact]
    public void Actualizar_partido_cancelado_lanza_BusinessRuleException()
    {
        // Arrange: partido cancelado.
        var partido = new Partido(DateTime.UtcNow, 1, 2);
        partido.CambiarEstado(EstadoPartido.Cancelado);

        // Act
        var act = () => partido.Actualizar(DateTime.UtcNow.AddDays(1), 1, 3, EstadoPartido.Programado);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*cancelado*");
    }

    [Fact]
    public void Cambiar_estado_a_Suspendido_limpia_los_marcadores()
    {
        // Arrange: partido jugado con resultado.
        var partido = new Partido(DateTime.UtcNow, 1, 2);
        partido.RegistrarResultado(2, 1);

        // Act: lo suspendemos (p. ej. por irregularidad → debe replanteársse).
        partido.CambiarEstado(EstadoPartido.Suspendido);

        // Assert: el marcador desaparece para mantener la consistencia con el CHECK
        //         "si Estado=Jugado los marcadores deben existir".
        partido.LocalScore.Should().BeNull();
        partido.VisitanteScore.Should().BeNull();
        partido.Estado.Should().Be(EstadoPartido.Suspendido);
    }
}
