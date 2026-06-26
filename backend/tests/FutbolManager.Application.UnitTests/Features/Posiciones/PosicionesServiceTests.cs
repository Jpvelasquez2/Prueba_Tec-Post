using System.Reflection;
using FluentAssertions;
using FutbolManager.Application.Common.Interfaces;
using FutbolManager.Application.Features.Posiciones.Services;
using FutbolManager.Domain.Common;
using FutbolManager.Domain.Entities;
using FutbolManager.Domain.Enums;
using FutbolManager.Domain.Posiciones;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FutbolManager.Application.UnitTests.Features.Posiciones;

/// <summary>
/// Pruebas del orquestador <see cref="PosicionesService"/>.
/// Demostramos el uso de <b>Moq</b> sobre los puertos (repositorios) y dejamos
/// el <see cref="CalculadorTablaPosiciones"/> real para no romper su contrato.
/// </summary>
public class PosicionesServiceTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Crea un <see cref="Equipo"/> con Id forzado. El setter de Id está
    /// protegido (lo asigna EF normalmente), pero en tests necesitamos
    /// controlar el valor para enlazar con los partidos.
    /// </summary>
    private static Equipo EquipoConId(int id, string nombre, string? ciudad = null, string? escudoUrl = null)
    {
        var equipo = new Equipo(nombre, ciudad, escudoUrl);
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(equipo, id);
        return equipo;
    }

    /// <summary>Crea un partido jugado con marcador, sin usar el constructor por defecto.</summary>
    private static Partido PartidoJugado(int localId, int visitanteId, int golesLocal, int golesVisitante)
    {
        var partido = new Partido(DateTime.UtcNow, localId, visitanteId);
        partido.RegistrarResultado(golesLocal, golesVisitante);
        return partido;
    }

    /// <summary>Construye un SUT con repositorios mockeados y calculador real.</summary>
    private static (PosicionesService Sut, Mock<IEquipoRepository> EquipoRepo, Mock<IPartidoRepository> PartidoRepo)
        BuildSut()
    {
        var equipoRepo = new Mock<IEquipoRepository>(MockBehavior.Strict);
        var partidoRepo = new Mock<IPartidoRepository>(MockBehavior.Strict);
        var calculador = new CalculadorTablaPosiciones();
        var sut = new PosicionesService(equipoRepo.Object, partidoRepo.Object, calculador, NullLogger<PosicionesService>.Instance);
        return (sut, equipoRepo, partidoRepo);
    }

    // -------------------------------------------------------------------------
    // GRUPO 1 — Comportamiento de orquestación
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CalcularAsync_consulta_a_los_repositorios_una_sola_vez()
    {
        // Arrange: ambos repos vacíos; verificamos que se llamen exactamente una vez.
        var (sut, equipoRepo, partidoRepo) = BuildSut();

        equipoRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Array.Empty<Equipo>());
        partidoRepo.Setup(r => r.GetJugadosAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Array.Empty<Partido>());

        // Act
        var resultado = await sut.CalcularAsync();

        // Assert: lista vacía + llamadas exactas a cada repo.
        resultado.Should().BeEmpty();
        equipoRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        partidoRepo.Verify(r => r.GetJugadosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalcularAsync_enriquece_la_salida_con_Ciudad_y_EscudoUrl()
    {
        // Arrange: equipo con metadatos. El calculador no los conoce; el
        // orquestador es responsable de unirlos con el DTO final.
        var (sut, equipoRepo, partidoRepo) = BuildSut();
        var equipo = EquipoConId(1, "Atlético", ciudad: "Bogotá", escudoUrl: "https://cdn/aux.png");

        equipoRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new[] { equipo });
        partidoRepo.Setup(r => r.GetJugadosAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Array.Empty<Partido>());

        // Act
        var resultado = await sut.CalcularAsync();

        // Assert
        var fila = resultado.Should().ContainSingle().Subject;
        fila.Ciudad.Should().Be("Bogotá");
        fila.EscudoUrl.Should().Be("https://cdn/aux.png");
    }

    [Fact]
    public async Task CalcularAsync_filtra_partidos_sin_marcador()
    {
        // Arrange: un partido jugado SIN marcador es un estado inconsistente
        // (no debería ocurrir, pero defendemos contra ello). El orquestador
        // debe descartarlo antes de llamar al calculador.
        var (sut, equipoRepo, partidoRepo) = BuildSut();

        var local = EquipoConId(1, "A");
        var visitante = EquipoConId(2, "B");

        // Creamos un partido marcado como Jugado pero sin marcadores: para esto,
        // usamos un partido en Programado y luego nos saltamos RegistrarResultado.
        // Con reflexión simulamos el inconsistente "Jugado sin marcador".
        var inconsistente = new Partido(DateTime.UtcNow, 1, 2);
        typeof(Partido).GetProperty(nameof(Partido.Estado))!
                       .SetValue(inconsistente, EstadoPartido.Jugado);

        equipoRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new[] { local, visitante });
        partidoRepo.Setup(r => r.GetJugadosAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new[] { inconsistente });

        // Act
        var resultado = await sut.CalcularAsync();

        // Assert: el partido inconsistente se ignoró, todos los acumulados quedan en 0.
        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(f => f.PJ == 0 && f.PTS == 0);
    }

    [Fact]
    public async Task CalcularAsync_construye_la_tabla_completa_combinando_datos_de_ambos_repos()
    {
        // Arrange: dos equipos con un partido jugado entre ellos.
        var (sut, equipoRepo, partidoRepo) = BuildSut();

        var a = EquipoConId(1, "Águilas", "Bogotá", "https://cdn/aguilas.png");
        var b = EquipoConId(2, "Zorros", "Cali", null);
        var partido = PartidoJugado(localId: 1, visitanteId: 2, golesLocal: 3, golesVisitante: 1);

        equipoRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new[] { a, b });
        partidoRepo.Setup(r => r.GetJugadosAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new[] { partido });

        // Act
        var resultado = await sut.CalcularAsync();

        // Assert: Águilas primero (3 PTS), Zorros segundo. Y los metadatos viajan.
        resultado.Should().HaveCount(2);

        resultado[0].Equipo.Should().Be("Águilas");
        resultado[0].PTS.Should().Be(3);
        resultado[0].GF.Should().Be(3);
        resultado[0].GC.Should().Be(1);
        resultado[0].DG.Should().Be(2);
        resultado[0].Ciudad.Should().Be("Bogotá");
        resultado[0].EscudoUrl.Should().Be("https://cdn/aguilas.png");

        resultado[1].Equipo.Should().Be("Zorros");
        resultado[1].PTS.Should().Be(0);
        resultado[1].PP.Should().Be(1);
        resultado[1].Ciudad.Should().Be("Cali");
        resultado[1].EscudoUrl.Should().BeNull();
    }

    [Fact]
    public async Task CalcularAsync_pasa_el_CancellationToken_a_los_repositorios()
    {
        // Arrange: el orquestador debe respetar la cancelación.
        var (sut, equipoRepo, partidoRepo) = BuildSut();
        using var cts = new CancellationTokenSource();

        equipoRepo.Setup(r => r.GetAllAsync(cts.Token))
                  .ReturnsAsync(Array.Empty<Equipo>());
        partidoRepo.Setup(r => r.GetJugadosAsync(cts.Token))
                   .ReturnsAsync(Array.Empty<Partido>());

        // Act
        await sut.CalcularAsync(cts.Token);

        // Assert: ambos repos recibieron exactamente el token que pasó el caller.
        equipoRepo.Verify(r => r.GetAllAsync(cts.Token), Times.Once);
        partidoRepo.Verify(r => r.GetJugadosAsync(cts.Token), Times.Once);
    }
}
