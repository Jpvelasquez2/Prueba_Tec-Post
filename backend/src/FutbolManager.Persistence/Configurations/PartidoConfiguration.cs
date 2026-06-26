using FutbolManager.Domain.Entities;
using FutbolManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutbolManager.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad <see cref="Partido"/>.
/// Refleja la estructura definida en <c>scripts/db/01_schema.sql</c>.
/// </summary>
public sealed class PartidoConfiguration : IEntityTypeConfiguration<Partido>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Partido> builder)
    {
        builder.ToTable("Partidos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Fecha)
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(p => p.LocalTeamId).IsRequired();
        builder.Property(p => p.VisitanteTeamId).IsRequired();

        // Los marcadores son TINYINT NULL en SQL hasta que el partido se juega.
        builder.Property(p => p.LocalScore).HasColumnType("tinyint");
        builder.Property(p => p.VisitanteScore).HasColumnType("tinyint");

        // Enum mapeado a TINYINT con HasConversion para que EF use los IDs
        // reservados (1=Programado, 2=Jugado, 3=Suspendido, 4=Cancelado).
        builder.Property(p => p.Estado)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(p => p.CreatedAt).HasColumnType("datetime2(3)");
        builder.Property(p => p.UpdatedAt).HasColumnType("datetime2(3)");

        // Relaciones (FK NO ACTION para reflejar el comportamiento del esquema:
        // no se puede borrar un equipo si tiene partidos asociados; esa regla
        // la traducimos a ConflictException en EquipoService.EliminarAsync).
        builder.HasOne(p => p.LocalTeam)
            .WithMany()
            .HasForeignKey(p => p.LocalTeamId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.VisitanteTeam)
            .WithMany()
            .HasForeignKey(p => p.VisitanteTeamId)
            .OnDelete(DeleteBehavior.NoAction);

        // Espejo de los índices definidos en SQL para que EF los considere
        // al generar planes (no son redundantes en runtime — son metadata).
        builder.HasIndex(p => p.Fecha);
        builder.HasIndex(p => p.LocalTeamId);
        builder.HasIndex(p => p.VisitanteTeamId);
        builder.HasIndex(p => p.Estado);
    }
}
