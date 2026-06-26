/* =============================================================================
   03_views.sql
   -----------------------------------------------------------------------------
   Vista y procedimiento almacenado para la tabla de posiciones.

   Diseño:
     - vw_TablaPosiciones: agregados por equipo, SIN ordenar (las vistas en
       SQL Server no garantizan orden).
     - sp_ObtenerTablaPosiciones: aplica el orden por desempates.

   Desempates aplicados:
       1) PTS desc
       2) DG  desc  (diferencia de goles)
       3) GF  desc  (goles a favor)

   Reglas de puntuación:
       Victoria = 3 puntos
       Empate   = 1 punto
       Derrota  = 0 puntos

   Solo se contabilizan partidos con EstadoId = 2 (Jugado).
   ============================================================================= */

USE [FutbolManagerDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =============================================================================
   1) Vista de la tabla de posiciones
   -----------------------------------------------------------------------------
   Estrategia:
     - Por cada partido jugado se generan DOS filas en un CTE unificado:
       una desde el punto de vista del local, otra desde el del visitante.
     - Sobre ese conjunto se agrega por EquipoId.
     - Se hace LEFT JOIN con Equipos para que aparezcan también los equipos
       que aún no han jugado (con todos los acumulados en 0).
   ============================================================================= */

CREATE OR ALTER VIEW dbo.vw_TablaPosiciones
AS
WITH ResultadosLocal AS
(
    /* Vista del equipo local en cada partido jugado. */
    SELECT
        p.LocalTeamId                                                AS EquipoId,
        CAST(p.LocalScore     AS INT)                                AS GF,
        CAST(p.VisitanteScore AS INT)                                AS GC,
        CASE WHEN p.LocalScore >  p.VisitanteScore THEN 1 ELSE 0 END AS PG,
        CASE WHEN p.LocalScore =  p.VisitanteScore THEN 1 ELSE 0 END AS PE,
        CASE WHEN p.LocalScore <  p.VisitanteScore THEN 1 ELSE 0 END AS PP
    FROM dbo.Partidos AS p
    WHERE p.EstadoId = 2  /* Jugado */
),
ResultadosVisitante AS
(
    /* Vista del equipo visitante en cada partido jugado. */
    SELECT
        p.VisitanteTeamId                                            AS EquipoId,
        CAST(p.VisitanteScore AS INT)                                AS GF,
        CAST(p.LocalScore     AS INT)                                AS GC,
        CASE WHEN p.VisitanteScore >  p.LocalScore THEN 1 ELSE 0 END AS PG,
        CASE WHEN p.VisitanteScore =  p.LocalScore THEN 1 ELSE 0 END AS PE,
        CASE WHEN p.VisitanteScore <  p.LocalScore THEN 1 ELSE 0 END AS PP
    FROM dbo.Partidos AS p
    WHERE p.EstadoId = 2
),
ResultadosUnificados AS
(
    SELECT EquipoId, GF, GC, PG, PE, PP FROM ResultadosLocal
    UNION ALL
    SELECT EquipoId, GF, GC, PG, PE, PP FROM ResultadosVisitante
),
Agregados AS
(
    /* Suma por equipo de todos los partidos jugados. */
    SELECT
        EquipoId,
        COUNT(*) AS PJ,
        SUM(PG)  AS PG,
        SUM(PE)  AS PE,
        SUM(PP)  AS PP,
        SUM(GF)  AS GF,
        SUM(GC)  AS GC
    FROM ResultadosUnificados
    GROUP BY EquipoId
)
SELECT
    e.Id                          AS EquipoId,
    e.Nombre                      AS Equipo,
    e.Ciudad,
    e.EscudoUrl,
    ISNULL(a.PJ, 0)               AS PJ,   /* Partidos Jugados */
    ISNULL(a.PG, 0)               AS PG,   /* Partidos Ganados */
    ISNULL(a.PE, 0)               AS PE,   /* Partidos Empatados */
    ISNULL(a.PP, 0)               AS PP,   /* Partidos Perdidos */
    ISNULL(a.GF, 0)               AS GF,   /* Goles a Favor */
    ISNULL(a.GC, 0)               AS GC,   /* Goles en Contra */
    ISNULL(a.GF, 0) - ISNULL(a.GC, 0)                       AS DG,  /* Diferencia de Goles */
    (ISNULL(a.PG, 0) * 3) + (ISNULL(a.PE, 0) * 1)           AS PTS  /* Puntos: V=3, E=1, D=0 */
FROM dbo.Equipos AS e
LEFT JOIN Agregados AS a ON a.EquipoId = e.Id;
GO

PRINT 'Vista vw_TablaPosiciones creada/actualizada.';
GO

/* =============================================================================
   2) Procedimiento almacenado: obtiene la tabla de posiciones ya ordenada
   -----------------------------------------------------------------------------
   Devuelve los equipos ordenados por:
       PTS desc, DG desc, GF desc, Equipo asc (alfabético como último recurso).

   El criterio head-to-head NO se aplica aquí (ver README.md).
   ============================================================================= */

CREATE OR ALTER PROCEDURE dbo.sp_ObtenerTablaPosiciones
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        EquipoId,
        Equipo,
        Ciudad,
        EscudoUrl,
        PJ, PG, PE, PP,
        GF, GC, DG, PTS,
        ROW_NUMBER() OVER (ORDER BY PTS DESC, DG DESC, GF DESC, Equipo ASC) AS Posicion
    FROM dbo.vw_TablaPosiciones
    ORDER BY Posicion;
END;
GO

PRINT 'Procedimiento sp_ObtenerTablaPosiciones creado/actualizado.';
GO

PRINT '03_views.sql aplicado correctamente.';
GO
