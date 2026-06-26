# FutbolManager.Persistence

**Acceso a datos sobre SQL Server con EF Core.** Implementa los repositorios definidos en Application.

## Responsabilidad

* **DbContext** (`ApplicationDbContext`) + `DbSet<>` por agregado.
* **Configurations Fluent API** por entidad (mejor que data annotations).
* **Repositorios** que implementan `IEquipoRepository`, `IPartidoRepository`, etc.
* **UnitOfWork** que envuelve `SaveChangesAsync` y publica eventos de dominio post-commit.
* **Interceptors**: auditoría (CreatedAt/UpdatedAt), soft-delete, despacho de eventos de dominio.
* **Migrations** versionadas.
* **Seeders** (datos iniciales: equipos demo, torneo demo).

## Restricciones

* No expone tipos de EF Core hacia capas superiores (`IQueryable<>` debe materializarse aquí).
* No contiene lógica de negocio: solo persistencia.
* Toda configuración Fluent API debe vivir en `Configurations/` (una clase por entidad).

## Estructura prevista

```
FutbolManager.Persistence/
├── Contexts/              # ApplicationDbContext
├── Configurations/        # IEntityTypeConfiguration<TEntity>
├── Repositories/
├── UnitOfWork/
├── Interceptors/
├── Migrations/            # generadas por dotnet ef
└── Seed/                  # DatabaseSeeder
```

## Comandos EF Core

```powershell
# Crear una migración (desde backend/)
dotnet ef migrations add NombreMigracion `
  --project src\FutbolManager.Persistence `
  --startup-project src\FutbolManager.Api

# Aplicar migraciones
dotnet ef database update `
  --project src\FutbolManager.Persistence `
  --startup-project src\FutbolManager.Api
```
