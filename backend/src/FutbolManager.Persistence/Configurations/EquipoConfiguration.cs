using FutbolManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutbolManager.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Equipo"/>.
/// Mapea las columnas tal como están definidas en <c>scripts/db/01_schema.sql</c>.
/// </summary>
public sealed class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Equipo> builder)
    {
        // Declarar el trigger AFTER UPDATE creado en SQL para que EF Core
        // no use la cláusula OUTPUT al hacer UPDATE (incompatible con triggers).
        builder.ToTable("Equipos", t => t.HasTrigger("TR_Equipos_AfterUpdate_UpdatedAt"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Ciudad)
            .HasMaxLength(100);

        builder.Property(e => e.EscudoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("datetime2(3)");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("datetime2(3)");

        // Espejo del UNIQUE definido en SQL (UQ_Equipos_Nombre).
        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}
