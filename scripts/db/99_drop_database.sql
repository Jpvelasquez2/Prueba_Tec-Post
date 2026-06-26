/* =============================================================================
   99_drop_database.sql
   -----------------------------------------------------------------------------
   Eliminación completa de FutbolManagerDb. **Solo entornos NO productivos.**

   Cierra todas las conexiones activas antes de eliminar para evitar el error
   "no se puede eliminar la base de datos porque está en uso".
   ============================================================================= */

USE [master];
GO

IF DB_ID(N'FutbolManagerDb') IS NOT NULL
BEGIN
    PRINT 'Cerrando conexiones a FutbolManagerDb...';
    ALTER DATABASE [FutbolManagerDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    PRINT 'Eliminando base de datos FutbolManagerDb...';
    DROP DATABASE [FutbolManagerDb];

    PRINT 'FutbolManagerDb eliminada.';
END
ELSE
BEGIN
    PRINT 'FutbolManagerDb no existe. Nada que eliminar.';
END
GO
