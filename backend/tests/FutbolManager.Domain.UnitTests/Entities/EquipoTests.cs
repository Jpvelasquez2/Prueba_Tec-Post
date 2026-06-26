using FluentAssertions;
using FutbolManager.Domain.Entities;
using FutbolManager.Domain.Exceptions;
using Xunit;

namespace FutbolManager.Domain.UnitTests.Entities;

/// <summary>
/// Pruebas sobre las invariantes de la entidad rica <see cref="Equipo"/>.
/// </summary>
public class EquipoTests
{
    // -------------------------------------------------------------------------
    // Construcción
    // -------------------------------------------------------------------------

    [Fact]
    public void Crear_equipo_aplica_trim_al_nombre()
    {
        // Arrange + Act: nombre con espacios alrededor.
        var equipo = new Equipo("  Real Madrid  ", null, null);

        // Assert: el dominio normaliza el nombre (sin espacios al borde).
        equipo.Nombre.Should().Be("Real Madrid");
    }

    [Fact]
    public void Crear_equipo_acepta_ciudad_y_escudo_opcionales_como_null()
    {
        // Arrange + Act
        var equipo = new Equipo("Liverpool", null, null);

        // Assert
        equipo.Ciudad.Should().BeNull();
        equipo.EscudoUrl.Should().BeNull();
    }

    [Fact]
    public void Crear_equipo_normaliza_ciudad_y_escudo_vacios_o_whitespace_a_null()
    {
        // Arrange: el front podría enviar cadenas vacías por error.
        // Act
        var equipo = new Equipo("Barcelona", "   ", "");

        // Assert: ambos quedan como null (la BD los almacena como NULL).
        equipo.Ciudad.Should().BeNull();
        equipo.EscudoUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_equipo_con_nombre_vacio_lanza_BusinessRuleException(string? nombre)
    {
        // Arrange + Act
        var act = () => new Equipo(nombre!, null, null);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*nombre*vacío*");
    }

    [Theory]
    [InlineData("ftp://malicious")]
    [InlineData("javascript:alert(1)")]
    [InlineData("not-a-url")]
    public void Crear_equipo_con_EscudoUrl_no_http_lanza_BusinessRuleException(string url)
    {
        // Arrange + Act
        var act = () => new Equipo("Equipo", null, url);

        // Assert: solo se aceptan http:// y https://.
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*http*");
    }

    [Theory]
    [InlineData("http://example.com/escudo.png")]
    [InlineData("https://cdn.example.com/img.svg")]
    public void Crear_equipo_acepta_EscudoUrl_http_y_https(string url)
    {
        // Arrange + Act
        var equipo = new Equipo("Equipo", null, url);

        // Assert
        equipo.EscudoUrl.Should().Be(url);
    }

    // -------------------------------------------------------------------------
    // Actualización
    // -------------------------------------------------------------------------

    [Fact]
    public void Actualizar_equipo_cambia_los_datos_y_refresca_UpdatedAt()
    {
        // Arrange
        var equipo = new Equipo("Original", "Original City", null);
        var antes = equipo.UpdatedAt;

        // Act
        equipo.Actualizar("Nuevo", "Nueva Ciudad", "https://x.com/e.png");

        // Assert
        equipo.Nombre.Should().Be("Nuevo");
        equipo.Ciudad.Should().Be("Nueva Ciudad");
        equipo.EscudoUrl.Should().Be("https://x.com/e.png");
        equipo.UpdatedAt.Should().BeOnOrAfter(antes);
    }

    [Fact]
    public void Actualizar_equipo_con_nombre_vacio_lanza_BusinessRuleException()
    {
        // Arrange
        var equipo = new Equipo("Original", null, null);

        // Act
        var act = () => equipo.Actualizar("", null, null);

        // Assert
        act.Should().Throw<BusinessRuleException>();
    }
}
