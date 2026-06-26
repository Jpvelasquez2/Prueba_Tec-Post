/* =============================================================================
   00_create_database.sql
   -----------------------------------------------------------------------------
   Crea la base de datos `FutbolManagerDb` si no existe.
   Ejecutar en el contexto de `master`.

   Uso (PowerShell + sqlcmd):
       sqlcmd -S localhost -E -i scripts\db\00_create_database.sql
   ============================================================================= */

USE [master];
GO

IF DB_ID(N'FutbolManagerDb') IS NULL
BEGIN
    PRINT 'Creando base de datos FutbolManagerDb...';
    CREATE DATABASE [FutbolManagerDb];
END
ELSE
BEGIN
    PRINT 'La base de datos FutbolManagerDb ya existe. Sin cambios.';
END
GO

/* Collation y opciones recomendadas para texto en español */
ALTER DATABASE [FutbolManagerDb] COLLATE Modern_Spanish_CI_AS;
GO

ALTER DATABASE [FutbolManagerDb] SET RECOVERY SIMPLE;       -- Suficiente para entornos de desarrollo
ALTER DATABASE [FutbolManagerDb] SET READ_COMMITTED_SNAPSHOT ON;
GO

PRINT 'Listo: FutbolManagerDb preparada.';
GO
