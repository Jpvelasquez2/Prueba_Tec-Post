using FutbolManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FutbolManager.Persistence.Contexts;

/// <summary>
/// <see cref="DbContext"/> de la aplicación. Expone los DbSet de Equipos y
/// Partidos, y descubre automáticamente las configuraciones Fluent API
/// declaradas en este ensamblado.
/// </summary>
/// <remarks>
/// <para>
/// El esquema lo gestionan los scripts en <c>scripts/db/</c> (fuente de
/// verdad). EF Core trabaja sobre el esquema ya existente; no generamos
/// migraciones para evitar duplicar la definición.
/// </para>
/// <para>
/// La tabla <c>EstadosPartido</c> es un catálogo del lado SQL: aquí no la
/// modelamos como entidad. El enum <see cref="Domain.Enums.EstadoPartido"/>
/// se mapea como TINYINT (ver <c>PartidoConfiguration</c>).
/// </para>
/// </remarks>
public sealed class ApplicationDbContext : DbContext
{
    /// <summary>Equipos persistidos.</summary>
    public DbSet<Equipo> Equipos => Set<Equipo>();

    /// <summary>Partidos persistidos.</summary>
    public DbSet<Partido> Partidos => Set<Partido>();

    /// <summary>DI constructor estándar de EF Core.</summary>
    /// <param name="options">Opciones configuradas en <c>Program.cs</c>.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Descubre todas las clases que implementan IEntityTypeConfiguration<>
        // y aplica su configuración (EquipoConfiguration, PartidoConfiguration).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
