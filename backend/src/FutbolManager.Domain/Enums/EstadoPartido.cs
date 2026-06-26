namespace FutbolManager.Domain.Enums;

/// <summary>
/// Estados posibles de un partido. Los valores numéricos coinciden con los
/// IDs reservados de la tabla maestra <c>EstadosPartido</c> en la BD.
/// </summary>
/// <remarks>
/// El subyacente es <see cref="byte"/> porque la columna SQL es <c>TINYINT</c>.
/// EF Core lo mapea mediante <c>HasConversion&lt;byte&gt;()</c>.
/// </remarks>
public enum EstadoPartido : byte
{
    /// <summary>Partido futuro pendiente de jugarse.</summary>
    Programado = 1,

    /// <summary>Partido finalizado con marcador final.</summary>
    Jugado = 2,

    /// <summary>Partido interrumpido; podrá reprogramarse.</summary>
    Suspendido = 3,

    /// <summary>Partido cancelado y no se reprograma.</summary>
    Cancelado = 4,
}
