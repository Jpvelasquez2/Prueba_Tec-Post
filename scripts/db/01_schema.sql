/* =============================================================================
   01_schema.sql
   -----------------------------------------------------------------------------
   Esquema de tablas, claves, restricciones, índices y triggers de auditoría
   para FutbolManagerDb.

   Diseño:
     - Tercera Forma Normal (3FN): cada atributo no-clave depende solo de la PK.
     - Catálogo de estados en tabla maestra (EstadosPartido) con IDs reservados.
     - Auditoría: CreatedAt vía DEFAULT, UpdatedAt vía trigger AFTER UPDATE.
     - Marcadores nullables (un partido programado todavía no tiene resultado).

   Idempotente: puede ejecutarse varias veces sin error.
   ============================================================================= */

USE [FutbolManagerDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =============================================================================
   1) Tabla: EstadosPartido (catálogo)
   -----------------------------------------------------------------------------
   Estados maestros de un partido. IDs reservados:
       1 = Programado   (partido futuro, sin marcador)
       2 = Jugado       (terminado, con marcador final)
       3 = Suspendido   (interrumpido, podrá reprogramarse)
       4 = Cancelado    (no se jugará y no se reprograma)

   Los IDs son fijos para poder referenciarlos desde restricciones CHECK
   sin recurrir a subqueries (no permitidas en CHECK).
   ============================================================================= */
IF OBJECT_ID(N'dbo.EstadosPartido', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EstadosPartido
    (
        Id          TINYINT         NOT NULL,
        Codigo      VARCHAR(20)     NOT NULL,
        Descripcion NVARCHAR(100)   NOT NULL,
        CreatedAt   DATETIME2(3)    NOT NULL CONSTRAINT DF_EstadosPartido_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_EstadosPartido          PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_EstadosPartido_Codigo   UNIQUE (Codigo)
    );

    PRINT 'Tabla EstadosPartido creada.';
END
GO

/* =============================================================================
   2) Tabla: Equipos
   -----------------------------------------------------------------------------
   Equipos participantes. El nombre es único (no puede haber dos equipos con el
   mismo nombre exacto). Ciudad y EscudoUrl son opcionales.
   ============================================================================= */
IF OBJECT_ID(N'dbo.Equipos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Equipos
    (
        Id          INT             IDENTITY(1, 1) NOT NULL,
        Nombre      NVARCHAR(100)   NOT NULL,
        Ciudad      NVARCHAR(100)   NULL,
        EscudoUrl   NVARCHAR(500)   NULL,
        CreatedAt   DATETIME2(3)    NOT NULL CONSTRAINT DF_Equipos_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2(3)    NOT NULL CONSTRAINT DF_Equipos_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Equipos                 PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Equipos_Nombre          UNIQUE (Nombre),
        CONSTRAINT CK_Equipos_NombreNoVacio   CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Equipos_EscudoUrlValido CHECK (
            EscudoUrl IS NULL
            OR EscudoUrl LIKE 'http://%'
            OR EscudoUrl LIKE 'https://%'
        )
    );

    PRINT 'Tabla Equipos creada.';
END
GO

/* =============================================================================
   3) Tabla: Partidos
   -----------------------------------------------------------------------------
   Enfrentamientos entre dos equipos. Los marcadores son nullables porque un
   partido recién programado todavía no tiene resultado.

   Reglas (expresadas como CHECK):
     - LocalTeamId <> VisitanteTeamId (un equipo no juega contra sí mismo).
     - Marcadores >= 0.
     - Si Estado = Jugado (EstadoId = 2), ambos marcadores deben estar
       informados.

   FKs con ON DELETE NO ACTION para evitar borrado en cascada accidental
   (un equipo no se elimina mientras tenga partidos asociados).
   ============================================================================= */
IF OBJECT_ID(N'dbo.Partidos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidos
    (
        Id              INT             IDENTITY(1, 1) NOT NULL,
        Fecha           DATETIME2(0)    NOT NULL,
        LocalTeamId     INT             NOT NULL,
        VisitanteTeamId INT             NOT NULL,
        LocalScore      TINYINT         NULL,
        VisitanteScore  TINYINT         NULL,
        EstadoId        TINYINT         NOT NULL CONSTRAINT DF_Partidos_EstadoId DEFAULT (1),  -- Programado
        CreatedAt       DATETIME2(3)    NOT NULL CONSTRAINT DF_Partidos_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2(3)    NOT NULL CONSTRAINT DF_Partidos_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        /* ---- Claves ---- */
        CONSTRAINT PK_Partidos
            PRIMARY KEY CLUSTERED (Id),

        CONSTRAINT FK_Partidos_Equipos_LocalTeamId
            FOREIGN KEY (LocalTeamId)     REFERENCES dbo.Equipos(Id)
            ON DELETE NO ACTION ON UPDATE NO ACTION,

        CONSTRAINT FK_Partidos_Equipos_VisitanteTeamId
            FOREIGN KEY (VisitanteTeamId) REFERENCES dbo.Equipos(Id)
            ON DELETE NO ACTION ON UPDATE NO ACTION,

        CONSTRAINT FK_Partidos_EstadosPartido_EstadoId
            FOREIGN KEY (EstadoId)        REFERENCES dbo.EstadosPartido(Id)
            ON DELETE NO ACTION ON UPDATE NO ACTION,

        /* ---- Reglas de negocio ---- */
        CONSTRAINT CK_Partidos_EquiposDistintos
            CHECK (LocalTeamId <> VisitanteTeamId),

        CONSTRAINT CK_Partidos_LocalScoreNoNegativo
            CHECK (LocalScore IS NULL OR LocalScore >= 0),

        CONSTRAINT CK_Partidos_VisitanteScoreNoNegativo
            CHECK (VisitanteScore IS NULL OR VisitanteScore >= 0),

        /* Si el partido está "Jugado" (EstadoId = 2), ambos marcadores deben existir. */
        CONSTRAINT CK_Partidos_JugadoTieneResultado
            CHECK (
                EstadoId <> 2
                OR (LocalScore IS NOT NULL AND VisitanteScore IS NOT NULL)
            )
    );

    PRINT 'Tabla Partidos creada.';
END
GO

/* =============================================================================
   4) Índices no únicos (los UNIQUE se crearon como restricciones arriba)
   ----------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Partidos_Fecha' AND object_id = OBJECT_ID(N'dbo.Partidos'))
    CREATE NONCLUSTERED INDEX IX_Partidos_Fecha
        ON dbo.Partidos (Fecha);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Partidos_LocalTeamId' AND object_id = OBJECT_ID(N'dbo.Partidos'))
    CREATE NONCLUSTERED INDEX IX_Partidos_LocalTeamId
        ON dbo.Partidos (LocalTeamId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Partidos_VisitanteTeamId' AND object_id = OBJECT_ID(N'dbo.Partidos'))
    CREATE NONCLUSTERED INDEX IX_Partidos_VisitanteTeamId
        ON dbo.Partidos (VisitanteTeamId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Partidos_EstadoId' AND object_id = OBJECT_ID(N'dbo.Partidos'))
    CREATE NONCLUSTERED INDEX IX_Partidos_EstadoId
        ON dbo.Partidos (EstadoId);

/* Índice filtrado optimizado para la vista de posiciones:
   solo cubre los partidos Jugados (EstadoId = 2) e incluye los marcadores
   como columnas no-clave para permitir un index-only scan. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Partidos_Jugados' AND object_id = OBJECT_ID(N'dbo.Partidos'))
    CREATE NONCLUSTERED INDEX IX_Partidos_Jugados
        ON dbo.Partidos (LocalTeamId, VisitanteTeamId)
        INCLUDE (LocalScore, VisitanteScore)
        WHERE EstadoId = 2;

PRINT 'Índices verificados/creados.';
GO

/* =============================================================================
   5) Triggers de auditoría — mantener UpdatedAt automáticamente
   -----------------------------------------------------------------------------
   Cada vez que se actualiza una fila, el trigger refresca UpdatedAt con la
   hora UTC actual. CreatedAt nunca se modifica.
   ============================================================================= */

IF OBJECT_ID(N'dbo.TR_Equipos_AfterUpdate_UpdatedAt', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_Equipos_AfterUpdate_UpdatedAt;
GO
CREATE TRIGGER dbo.TR_Equipos_AfterUpdate_UpdatedAt
    ON dbo.Equipos
    AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    /* Evita recursión infinita si el propio trigger dispara otro UPDATE. */
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE e
       SET UpdatedAt = SYSUTCDATETIME()
      FROM dbo.Equipos AS e
     INNER JOIN inserted AS i ON e.Id = i.Id;
END;
GO

IF OBJECT_ID(N'dbo.TR_Partidos_AfterUpdate_UpdatedAt', N'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_Partidos_AfterUpdate_UpdatedAt;
GO
CREATE TRIGGER dbo.TR_Partidos_AfterUpdate_UpdatedAt
    ON dbo.Partidos
    AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE p
       SET UpdatedAt = SYSUTCDATETIME()
      FROM dbo.Partidos AS p
     INNER JOIN inserted AS i ON p.Id = i.Id;
END;
GO

PRINT 'Triggers de auditoría creados.';
GO

PRINT '01_schema.sql aplicado correctamente.';
GO
