namespace FutbolManager.Domain.Posiciones;

/// <summary>
/// Calculadora pura de la tabla de posiciones de un torneo de fútbol.
/// </summary>
/// <remarks>
/// <para>
/// <b>Independencia.</b> Esta clase no depende de repositorios, EF Core,
/// logging ni frameworks de IO. Recibe los datos por parámetro y devuelve
/// un resultado. Es <i>determinística</i> y <i>trivialmente testeable</i>:
/// para una entrada dada siempre produce la misma salida.
/// </para>
/// <para>
/// <b>Reglas de puntuación.</b> Victoria = 3 pts, Empate = 1 pt, Derrota = 0 pts.
/// </para>
/// <para>
/// <b>Orden de salida</b> (de mayor a menor prioridad):
/// PTS desc → DG desc → GF desc → Nombre asc.
/// El último criterio (alfabético) garantiza un <i>orden total</i>
/// reproducible cuando todos los anteriores coinciden.
/// </para>
/// <para>
/// <b>Head-to-head NO implementado.</b> El cuarto desempate clásico
/// (resultado directo entre equipos empatados) requiere un sub-cálculo
/// recursivo sobre subconjuntos dinámicos de partidos; ver
/// <c>scripts/db/README.md</c> para la justificación detallada.
/// </para>
/// <para>
/// <b>Complejidad.</b> Con <i>N</i> equipos y <i>M</i> resultados:
/// O(<i>N</i> + <i>M</i> + <i>N</i> log <i>N</i>). El término dominante
/// suele ser <i>M</i> (cantidad de partidos) y, en torneos pequeños, el
/// log <i>N</i> del ordenamiento final.
/// </para>
/// </remarks>
public sealed class CalculadorTablaPosiciones
{
    // Constantes de negocio. Se definen aquí (no en config) porque son
    // reglas universales del fútbol moderno: 3-1-0 desde 1994.
    private const int PuntosPorVictoria = 3;
    private const int PuntosPorEmpate   = 1;
    private const int PuntosPorDerrota  = 0; // Documentado por claridad, no se usa en el cálculo.

    /// <summary>
    /// Calcula la tabla de posiciones a partir de los equipos participantes
    /// y los resultados de los partidos jugados.
    /// </summary>
    /// <param name="equipos">Equipos del torneo. Los duplicados por Id se ignoran.</param>
    /// <param name="resultados">Resultados de los partidos ya jugados.</param>
    /// <returns>
    /// Tabla ordenada y numerada. Incluye también a los equipos sin partidos
    /// jugados, que aparecen al final con todos los acumulados en 0.
    /// </returns>
    /// <exception cref="ArgumentNullException">Si alguna de las colecciones es <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Si algún resultado tiene goles negativos.</exception>
    public IReadOnlyList<FilaPosicion> Calcular(
        IEnumerable<EquipoParticipante> equipos,
        IEnumerable<ResultadoPartido> resultados)
    {
        ArgumentNullException.ThrowIfNull(equipos);
        ArgumentNullException.ThrowIfNull(resultados);

        // =====================================================================
        // PASO 1 — Inicializar el acumulador.
        // ---------------------------------------------------------------------
        // Construimos un diccionario id → estado vacío para cada equipo.
        // El diccionario nos da acceso O(1) en el bucle siguiente y garantiza
        // que TODOS los equipos aparezcan en la tabla final, incluso aquellos
        // que aún no han jugado (con PJ=PG=…=0).
        //
        // Si recibimos equipos duplicados por id, los descartamos para evitar
        // resultados acumulados dos veces (último gana en términos de Nombre).
        // =====================================================================
        var acumuladores = new Dictionary<int, Acumulador>();
        foreach (var equipo in equipos)
        {
            // Evitamos sobrescribir un acumulador ya iniciado (mantiene el primer Nombre encontrado).
            if (!acumuladores.ContainsKey(equipo.Id))
                acumuladores[equipo.Id] = new Acumulador(equipo.Nombre);
        }

        // =====================================================================
        // PASO 2 — Recorrer cada resultado y proyectar dos perspectivas.
        // ---------------------------------------------------------------------
        // Un partido es un evento simétrico: lo que un equipo anota como
        // "goles a favor" el otro lo anota como "goles en contra". En lugar
        // de duplicar lógica, lo proyectamos como dos llamadas a un mismo
        // método de acumulación, invirtiendo los roles.
        //
        // Validamos goles no negativos como defensa en profundidad
        // (la API ya valida con DataAnnotations, pero el calculador es
        // reutilizable y no puede asumir limpieza río arriba).
        // =====================================================================
        foreach (var partido in resultados)
        {
            if (partido.GolesLocal < 0 || partido.GolesVisitante < 0)
                throw new ArgumentOutOfRangeException(nameof(resultados),
                    "Los goles no pueden ser negativos.");

            // Si un resultado referencia un equipo que no está en la lista
            // de participantes, simplemente lo ignoramos en lugar de fallar.
            // Es una decisión defensiva: hace al calculador tolerante a datos
            // parcialmente inconsistentes (p. ej. equipo borrado en otra parte).
            AcumularPerspectiva(
                acumuladores,
                equipoId:     partido.EquipoLocalId,
                golesFavor:   partido.GolesLocal,
                golesContra:  partido.GolesVisitante);

            AcumularPerspectiva(
                acumuladores,
                equipoId:     partido.EquipoVisitanteId,
                golesFavor:   partido.GolesVisitante,
                golesContra:  partido.GolesLocal);
        }

        // =====================================================================
        // PASO 3 — Derivar columnas calculadas (DG y PTS).
        // ---------------------------------------------------------------------
        // Convertimos cada acumulador en una "tupla pre-ordenamiento" donde ya
        // existen DG y PTS para poder ordenar en O(1) por comparación.
        // Esto evita recalcular dentro del comparador (cosa que multiplicaría
        // el costo en sorts grandes).
        // =====================================================================
        var filasPreOrdenadas = acumuladores
            .Select(par => ProyectarFila(par.Key, par.Value))
            .ToList();

        // =====================================================================
        // PASO 4 — Ordenar por criterios de desempate.
        // ---------------------------------------------------------------------
        // Orden lexicográfico: PTS desc → DG desc → GF desc → Nombre asc.
        // Para el nombre usamos InvariantCultureIgnoreCase: respeta el orden
        // alfabético del español invariante (Á cuenta como A), sin depender de
        // la cultura del SO de despliegue. Si usáramos OrdinalIgnoreCase, "Á"
        // (U+00C1) quedaría DESPUÉS de "Z" (U+005A), lo que no es el orden
        // alfabético esperado por un humano.
        // =====================================================================
        var filasOrdenadas = filasPreOrdenadas
            .OrderByDescending(f => f.Pts)
            .ThenByDescending(f => f.Dg)
            .ThenByDescending(f => f.Gf)
            .ThenBy(f => f.Nombre, StringComparer.InvariantCultureIgnoreCase)
            .ToList();

        // =====================================================================
        // PASO 5 — Asignar la posición final.
        // ---------------------------------------------------------------------
        // Posicion = índice + 1. Se asigna DESPUÉS de ordenar para que la
        // numeración refleje el ranking real.
        // =====================================================================
        var resultado = new FilaPosicion[filasOrdenadas.Count];
        for (var i = 0; i < filasOrdenadas.Count; i++)
        {
            var fila = filasOrdenadas[i];
            resultado[i] = new FilaPosicion(
                Posicion: i + 1,
                EquipoId: fila.EquipoId,
                Equipo:   fila.Nombre,
                Pj:       fila.Pj,
                Pg:       fila.Pg,
                Pe:       fila.Pe,
                Pp:       fila.Pp,
                Gf:       fila.Gf,
                Gc:       fila.Gc,
                Dg:       fila.Dg,
                Pts:      fila.Pts);
        }

        return resultado;
    }

    /// <summary>
    /// Acumula los goles y el resultado de un partido para un equipo concreto.
    /// </summary>
    /// <remarks>
    /// Es el corazón del cálculo. Lo invocamos dos veces por partido (una por
    /// equipo) con los goles "rotados". Si el equipo no está registrado, no
    /// hacemos nada (ver justificación en el paso 2).
    /// </remarks>
    private static void AcumularPerspectiva(
        IDictionary<int, Acumulador> acumuladores,
        int equipoId,
        int golesFavor,
        int golesContra)
    {
        if (!acumuladores.TryGetValue(equipoId, out var acumulador)) return;

        // PJ: cualquier partido jugado suma 1.
        acumulador.Pj++;

        // GF y GC: agregación directa.
        acumulador.Gf += golesFavor;
        acumulador.Gc += golesContra;

        // Resultado: una sola comparación define PG / PE / PP.
        // Solo uno de los tres contadores se incrementa por partido,
        // por lo que se preserva la invariante PG + PE + PP = PJ.
        if (golesFavor > golesContra)
            acumulador.Pg++;
        else if (golesFavor == golesContra)
            acumulador.Pe++;
        else
            acumulador.Pp++;
    }

    /// <summary>
    /// Convierte un acumulador en una fila pre-ordenamiento con DG y PTS ya calculados.
    /// </summary>
    private static FilaPreOrdenamiento ProyectarFila(int equipoId, Acumulador a)
    {
        // Diferencia de goles: puede ser negativa.
        var dg = a.Gf - a.Gc;

        // Puntos: V × 3 + E × 1.  (D × 0 está implícito y omitido).
        var pts = (a.Pg * PuntosPorVictoria) + (a.Pe * PuntosPorEmpate);

        return new FilaPreOrdenamiento(equipoId, a.Nombre, a.Pj, a.Pg, a.Pe, a.Pp, a.Gf, a.Gc, dg, pts);
    }

    // -------------------------------------------------------------------------
    // Tipos internos. Privados porque solo tienen sentido durante el cálculo.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Acumulador mutable usado durante el recorrido de partidos.
    /// </summary>
    /// <remarks>
    /// Es una clase (no <c>struct</c> ni <c>record</c>) porque necesitamos
    /// mutación in-place dentro del diccionario sin reasignar el valor en
    /// cada incremento, lo que evita copiar el struct repetidamente.
    /// </remarks>
    private sealed class Acumulador
    {
        public string Nombre { get; }
        public int Pj, Pg, Pe, Pp, Gf, Gc;

        public Acumulador(string nombre) { Nombre = nombre; }
    }

    /// <summary>
    /// Tupla intermedia con DG y PTS ya calculados, lista para ordenar.
    /// </summary>
    private sealed record FilaPreOrdenamiento(
        int EquipoId,
        string Nombre,
        int Pj,
        int Pg,
        int Pe,
        int Pp,
        int Gf,
        int Gc,
        int Dg,
        int Pts);
}
