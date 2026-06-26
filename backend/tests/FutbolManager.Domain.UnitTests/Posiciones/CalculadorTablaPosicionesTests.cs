using FluentAssertions;
using FutbolManager.Domain.Posiciones;
using Xunit;

namespace FutbolManager.Domain.UnitTests.Posiciones;

/// <summary>
/// Pruebas unitarias del algoritmo puro de cálculo de tabla de posiciones.
///
/// El SUT es una clase sin dependencias externas, por lo que NO usamos Moq aquí:
/// instanciamos el calculador directamente. La cobertura se concentra en este
/// archivo porque toda la lógica de negocio del scoring vive en una sola clase.
/// </summary>
public class CalculadorTablaPosicionesTests
{
    // -------------------------------------------------------------------------
    // Helpers compartidos: hacen los Arrange más legibles en cada test.
    // -------------------------------------------------------------------------

    private static CalculadorTablaPosiciones Sut() => new();

    private static EquipoParticipante Equipo(int id, string nombre) => new(id, nombre);

    private static ResultadoPartido Resultado(int local, int visitante, int gl, int gv)
        => new(local, visitante, gl, gv);

    // =========================================================================
    // GRUPO 1 — Casos base
    // =========================================================================

    [Fact]
    public void Sin_equipos_y_sin_partidos_devuelve_lista_vacia()
    {
        // Arrange: SUT puro, sin datos en absoluto.
        var sut = Sut();

        // Act: invocar con dos colecciones vacías.
        var tabla = sut.Calcular(Array.Empty<EquipoParticipante>(), Array.Empty<ResultadoPartido>());

        // Assert: el resultado debe ser una colección vacía, no null.
        tabla.Should().NotBeNull();
        tabla.Should().BeEmpty();
    }

    [Fact]
    public void Con_equipos_pero_sin_partidos_todos_los_acumulados_son_cero()
    {
        // Arrange: dos equipos sin partidos jugados todavía (arranque del torneo).
        var equipos = new[] { Equipo(1, "Atlético"), Equipo(2, "Bayern") };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, Array.Empty<ResultadoPartido>());

        // Assert: ambos aparecen con todos los acumulados en 0.
        tabla.Should().HaveCount(2);
        tabla.Should().OnlyContain(f =>
            f.Pj == 0 && f.Pg == 0 && f.Pe == 0 && f.Pp == 0 &&
            f.Gf == 0 && f.Gc == 0 && f.Dg == 0 && f.Pts == 0);
    }

    // =========================================================================
    // GRUPO 2 — Victoria
    // =========================================================================

    [Fact]
    public void Victoria_local_otorga_3_PTS_y_1_PG_al_ganador()
    {
        // Arrange: el local A gana 2-1 al visitante B.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B") };
        var resultados = new[] { Resultado(local: 1, visitante: 2, gl: 2, gv: 1) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: el local A debe tener 3 PTS y 1 PG; el visitante B 0 PTS y 1 PP.
        var a = tabla.Single(f => f.EquipoId == 1);
        var b = tabla.Single(f => f.EquipoId == 2);

        a.Pts.Should().Be(3);
        a.Pg.Should().Be(1);
        a.Pe.Should().Be(0);
        a.Pp.Should().Be(0);

        b.Pts.Should().Be(0);
        b.Pp.Should().Be(1);
        b.Pg.Should().Be(0);
    }

    [Fact]
    public void Victoria_visitante_otorga_3_PTS_y_1_PG_al_visitante()
    {
        // Arrange: el visitante gana 0-1 (perspectiva inversa al test anterior).
        var equipos = new[] { Equipo(1, "Local"), Equipo(2, "Visitante") };
        var resultados = new[] { Resultado(local: 1, visitante: 2, gl: 0, gv: 1) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: el visitante tiene 3 PTS; el local 0 PTS.
        tabla.Single(f => f.EquipoId == 2).Pts.Should().Be(3);
        tabla.Single(f => f.EquipoId == 2).Pg.Should().Be(1);
        tabla.Single(f => f.EquipoId == 1).Pts.Should().Be(0);
        tabla.Single(f => f.EquipoId == 1).Pp.Should().Be(1);
    }

    // =========================================================================
    // GRUPO 3 — Empate
    // =========================================================================

    [Fact]
    public void Empate_otorga_1_PTS_a_cada_equipo()
    {
        // Arrange: empate 1-1.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B") };
        var resultados = new[] { Resultado(1, 2, 1, 1) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: ambos suman 1 PE y 1 PTS.
        tabla.Should().OnlyContain(f => f.Pts == 1 && f.Pe == 1 && f.Pg == 0 && f.Pp == 0);
    }

    [Fact]
    public void Empate_sin_goles_se_contabiliza_como_empate()
    {
        // Arrange: empate 0-0 (caso esquina: ningún gol).
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B") };
        var resultados = new[] { Resultado(1, 2, 0, 0) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: cada uno tiene PE=1 y PTS=1, y GF=GC=0.
        tabla.Should().OnlyContain(f =>
            f.Pe == 1 && f.Pts == 1 && f.Gf == 0 && f.Gc == 0 && f.Dg == 0);
    }

    // =========================================================================
    // GRUPO 4 — Derrota
    // =========================================================================

    [Fact]
    public void Derrota_otorga_0_PTS_y_1_PP_al_perdedor()
    {
        // Arrange: el local pierde 0-3 en casa.
        var equipos = new[] { Equipo(1, "Casa"), Equipo(2, "Visita") };
        var resultados = new[] { Resultado(1, 2, 0, 3) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: el local sumó PP=1 y 0 PTS; el visitante 1 PG y 3 PTS.
        var casa = tabla.Single(f => f.EquipoId == 1);
        casa.Pp.Should().Be(1);
        casa.Pts.Should().Be(0);
        casa.Pg.Should().Be(0);
        casa.Pe.Should().Be(0);
    }

    // =========================================================================
    // GRUPO 5 — Goles a favor, en contra, y diferencia
    // =========================================================================

    [Fact]
    public void Goles_se_acumulan_segun_la_perspectiva_de_cada_equipo()
    {
        // Arrange: en un 3-1, el local mete 3 (GF) y recibe 1 (GC); inversa para el visitante.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B") };
        var resultados = new[] { Resultado(1, 2, 3, 1) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: A tiene GF=3 / GC=1; B tiene GF=1 / GC=3.
        var a = tabla.Single(f => f.EquipoId == 1);
        var b = tabla.Single(f => f.EquipoId == 2);
        a.Gf.Should().Be(3);
        a.Gc.Should().Be(1);
        b.Gf.Should().Be(1);
        b.Gc.Should().Be(3);
    }

    [Fact]
    public void Goles_se_suman_a_lo_largo_de_varios_partidos()
    {
        // Arrange: equipo A juega 3 partidos: 3-1, 0-0 y 1-2.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B"), Equipo(3, "C") };
        var resultados = new[]
        {
            Resultado(1, 2, 3, 1),   // A 3-1 B
            Resultado(1, 3, 0, 0),   // A 0-0 C
            Resultado(2, 1, 2, 1),   // B 2-1 A  (perspectiva: A jugó de visitante, perdió 1-2)
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: A jugó 3 partidos; GF = 3+0+1 = 4; GC = 1+0+2 = 3; DG = 1.
        var a = tabla.Single(f => f.EquipoId == 1);
        a.Pj.Should().Be(3);
        a.Gf.Should().Be(4);
        a.Gc.Should().Be(3);
        a.Dg.Should().Be(1);
    }

    [Fact]
    public void Diferencia_de_goles_es_GF_menos_GC()
    {
        // Arrange: 4-1 (DG = +3) en un único partido.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B") };
        var resultados = new[] { Resultado(1, 2, 4, 1) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: A tiene DG=+3; B tiene DG=-3.
        tabla.Single(f => f.EquipoId == 1).Dg.Should().Be(3);
        tabla.Single(f => f.EquipoId == 2).Dg.Should().Be(-3);
    }

    [Fact]
    public void Diferencia_de_goles_puede_ser_negativa()
    {
        // Arrange: el equipo siempre pierde por goleada.
        var equipos = new[] { Equipo(1, "Débil"), Equipo(2, "Fuerte") };
        var resultados = new[] { Resultado(1, 2, 0, 5) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: DG negativa es válida y se refleja como entero negativo.
        tabla.Single(f => f.EquipoId == 1).Dg.Should().Be(-5);
    }

    // =========================================================================
    // GRUPO 6 — Invariantes
    // =========================================================================

    [Fact]
    public void Invariante_PJ_es_igual_a_PG_mas_PE_mas_PP()
    {
        // Arrange: torneo con 4 partidos mixtos.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B"), Equipo(3, "C") };
        var resultados = new[]
        {
            Resultado(1, 2, 2, 1),   // A gana
            Resultado(2, 3, 0, 0),   // empate
            Resultado(3, 1, 1, 2),   // A gana
            Resultado(2, 1, 3, 0),   // B gana
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: para cada equipo, PJ = PG + PE + PP siempre.
        tabla.Should().OnlyContain(f => f.Pj == f.Pg + f.Pe + f.Pp);
    }

    [Fact]
    public void Puntos_se_calculan_como_PG_por_3_mas_PE_por_1()
    {
        // Arrange: 2 victorias + 1 empate + 1 derrota = 7 PTS.
        var equipos = new[] { Equipo(1, "Mix"), Equipo(2, "Op") };
        var resultados = new[]
        {
            Resultado(1, 2, 3, 0),  // Mix gana
            Resultado(1, 2, 1, 1),  // empate
            Resultado(1, 2, 2, 0),  // Mix gana
            Resultado(1, 2, 0, 1),  // Mix pierde
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: Mix tiene 2*3 + 1*1 + 1*0 = 7 PTS.
        tabla.Single(f => f.EquipoId == 1).Pts.Should().Be(7);
    }

    // =========================================================================
    // GRUPO 7 — Ordenamiento y desempates
    // =========================================================================

    [Fact]
    public void Tabla_se_ordena_por_PTS_descendente()
    {
        // Arrange: tres equipos con distinto puntaje.
        var equipos = new[] { Equipo(1, "Bajo"), Equipo(2, "Alto"), Equipo(3, "Medio") };
        var resultados = new[]
        {
            Resultado(2, 1, 5, 0),   // Alto gana → 3 PTS
            Resultado(3, 1, 1, 1),   // Medio empata → 1 PTS
            Resultado(1, 3, 1, 1),   // Bajo empata → 1 PTS (pero peor DG/GF acumulados)
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: el primero es el que tiene más PTS.
        tabla[0].Equipo.Should().Be("Alto");
        tabla[0].Pts.Should().Be(3);
    }

    [Fact]
    public void Cuando_PTS_son_iguales_desempata_por_DG_descendente()
    {
        // Arrange: dos equipos con 3 PTS pero distinta DG.
        var equipos = new[] { Equipo(1, "MejorDG"), Equipo(2, "PeorDG"), Equipo(3, "C") };
        var resultados = new[]
        {
            Resultado(1, 3, 5, 0),   // MejorDG gana 5-0 → DG +5
            Resultado(2, 3, 2, 1),   // PeorDG gana 2-1 → DG +1
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: ambos tienen 3 PTS, MejorDG debe estar primero.
        tabla[0].Equipo.Should().Be("MejorDG");
        tabla[1].Equipo.Should().Be("PeorDG");
    }

    [Fact]
    public void Cuando_PTS_y_DG_son_iguales_desempata_por_GF_descendente()
    {
        // Arrange: dos equipos con misma DG (+1) pero distinta cantidad de goles.
        var equipos = new[] { Equipo(1, "Goleador"), Equipo(2, "Modesto"), Equipo(3, "C") };
        var resultados = new[]
        {
            Resultado(1, 3, 4, 3),   // Goleador gana 4-3 → DG +1, GF=4
            Resultado(2, 3, 2, 1),   // Modesto  gana 2-1 → DG +1, GF=2
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: con DG idéntica el de más GF queda primero.
        tabla[0].Equipo.Should().Be("Goleador");
        tabla[1].Equipo.Should().Be("Modesto");
    }

    [Fact]
    public void Cuando_PTS_DG_y_GF_son_iguales_desempata_por_nombre_ascendente()
    {
        // Arrange: dos equipos completamente empatados en todos los criterios numéricos.
        // Esto fuerza el último tiebreaker (alfabético) para garantizar orden estable.
        var equipos = new[] { Equipo(1, "Zorro"), Equipo(2, "Águila"), Equipo(3, "M") };
        var resultados = new[]
        {
            Resultado(1, 3, 2, 1),   // Zorro  gana 2-1
            Resultado(2, 3, 2, 1),   // Águila gana 2-1 (mismo perfil)
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: con todo igualado, "Águila" precede a "Zorro" alfabéticamente.
        tabla[0].Equipo.Should().Be("Águila");
        tabla[1].Equipo.Should().Be("Zorro");
    }

    [Fact]
    public void Posicion_inicia_en_1_y_es_consecutiva()
    {
        // Arrange: cualquier conjunto no vacío.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B"), Equipo(3, "C") };
        var resultados = new[] { Resultado(1, 2, 3, 0) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: las posiciones son 1, 2, 3 sin huecos.
        tabla.Select(f => f.Posicion).Should().BeEquivalentTo(new[] { 1, 2, 3 }, opt => opt.WithStrictOrdering());
    }

    // =========================================================================
    // GRUPO 8 — Robustez / casos defensivos
    // =========================================================================

    [Fact]
    public void Resultado_que_referencia_equipo_no_registrado_se_ignora()
    {
        // Arrange: solo registramos el equipo 1; el resultado menciona al 99.
        var equipos = new[] { Equipo(1, "Único") };
        var resultados = new[] { Resultado(local: 1, visitante: 99, gl: 2, gv: 1) };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: el equipo 1 sí contabiliza su parte (1-0 victoria);
        //         el 99 no aparece en la tabla (no estaba registrado).
        tabla.Should().HaveCount(1);
        var unico = tabla.Single();
        unico.Pj.Should().Be(1);
        unico.Pg.Should().Be(1);
        unico.Gf.Should().Be(2);
        unico.Gc.Should().Be(1);
    }

    [Fact]
    public void Equipos_null_lanza_ArgumentNullException()
    {
        // Arrange
        var sut = Sut();

        // Act
        var act = () => sut.Calcular(equipos: null!, Array.Empty<ResultadoPartido>());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("equipos");
    }

    [Fact]
    public void Resultados_null_lanza_ArgumentNullException()
    {
        // Arrange
        var sut = Sut();

        // Act
        var act = () => sut.Calcular(Array.Empty<EquipoParticipante>(), resultados: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("resultados");
    }

    [Theory]
    [InlineData(-1, 0)]   // goles locales negativos
    [InlineData(0, -1)]   // goles visitantes negativos
    [InlineData(-1, -1)]  // ambos negativos
    public void Goles_negativos_lanzan_ArgumentOutOfRangeException(int gl, int gv)
    {
        // Arrange: SUT con un partido inválido.
        var equipos = new[] { Equipo(1, "A"), Equipo(2, "B") };
        var resultados = new[] { Resultado(1, 2, gl, gv) };
        var sut = Sut();

        // Act
        var act = () => sut.Calcular(equipos, resultados);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .Which.ParamName.Should().Be("resultados");
    }

    // =========================================================================
    // GRUPO 9 — Caso end-to-end (mini torneo)
    // =========================================================================

    [Fact]
    public void Mini_torneo_completo_produce_tabla_esperada()
    {
        // Arrange: 4 equipos, todos juegan contra todos una vez (6 partidos).
        // Diseñamos resultados para validar puntaje, ordenamiento y desempates.
        var equipos = new[]
        {
            Equipo(1, "Atlético"),
            Equipo(2, "Bayern"),
            Equipo(3, "City"),
            Equipo(4, "Dortmund"),
        };
        var resultados = new[]
        {
            Resultado(1, 2, 2, 1),  // Atlético  gana
            Resultado(1, 3, 1, 1),  // Empate
            Resultado(1, 4, 3, 0),  // Atlético  gana
            Resultado(2, 3, 1, 2),  // City      gana
            Resultado(2, 4, 2, 0),  // Bayern    gana
            Resultado(3, 4, 2, 2),  // Empate
        };
        var sut = Sut();

        // Act
        var tabla = sut.Calcular(equipos, resultados);

        // Assert: validar acumulados y posiciones exactas.
        //
        // Atlético: V V E → 7 PTS, GF=6, GC=2, DG=+4 → 1°
        // City:     V E E → 5 PTS, GF=5, GC=4, DG=+1 → 2°
        // Bayern:   V D D → 3 PTS, GF=4, GC=4, DG= 0 → 3°
        // Dortmund: D E D → 1 PTS, GF=2, GC=7, DG=-5 → 4°
        tabla.Should().HaveCount(4);

        tabla[0].Equipo.Should().Be("Atlético");
        tabla[0].Pts.Should().Be(7);
        tabla[0].Dg.Should().Be(4);

        tabla[1].Equipo.Should().Be("City");
        tabla[1].Pts.Should().Be(5);

        tabla[2].Equipo.Should().Be("Bayern");
        tabla[2].Pts.Should().Be(3);

        tabla[3].Equipo.Should().Be("Dortmund");
        tabla[3].Pts.Should().Be(1);

        // Y los marcadores PJ son consistentes (cada equipo jugó 3 partidos).
        tabla.Should().OnlyContain(f => f.Pj == 3);
    }
}
