/* =============================================================================
   02_seed.sql
   -----------------------------------------------------------------------------
   Datos maestros. Solo carga el catálogo EstadosPartido con IDs reservados.

   NO carga equipos ni partidos de ejemplo — eso es responsabilidad del
   DatabaseSeeder de la capa Persistence (modo Development).

   Idempotente: usa MERGE para insertar/actualizar sin duplicados.
   ============================================================================= */

USE [FutbolManagerDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT 'Cargando catálogo EstadosPartido...';

;WITH Origen (Id, Codigo, Descripcion) AS
(
    SELECT  1, 'Programado', N'Partido futuro pendiente de jugarse.'  UNION ALL
    SELECT  2, 'Jugado',     N'Partido finalizado con marcador final.' UNION ALL
    SELECT  3, 'Suspendido', N'Partido interrumpido; podrá reprogramarse.' UNION ALL
    SELECT  4, 'Cancelado',  N'Partido cancelado y no se reprograma.'
)
MERGE dbo.EstadosPartido AS destino
USING Origen           AS origen
   ON destino.Id = origen.Id
WHEN MATCHED THEN
    UPDATE SET
        Codigo      = origen.Codigo,
        Descripcion = origen.Descripcion
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Codigo, Descripcion)
    VALUES (origen.Id, origen.Codigo, origen.Descripcion);

PRINT CONCAT('EstadosPartido: ', (SELECT COUNT(*) FROM dbo.EstadosPartido), ' filas presentes.');
GO

PRINT '02_seed.sql aplicado correctamente.';
GO