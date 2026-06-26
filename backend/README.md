# FutbolManager — Backend

Solución ASP.NET Core 8 LTS basada en **Clean Architecture** (5 proyectos).

## Proyectos

```
backend/
├── FutbolManager.sln
├── Directory.Build.props          # Props comunes: TargetFramework, XML docs, warnings as errors
└── src/
    ├── FutbolManager.Api/         # Presentación / Composition Root
    ├── FutbolManager.Application/ # Casos de uso (CQRS, MediatR)
    ├── FutbolManager.Domain/      # Núcleo: entidades, value objects, reglas
    ├── FutbolManager.Infrastructure/ # Servicios técnicos (JWT, email, etc.)
    └── FutbolManager.Persistence/ # EF Core, DbContext, repositorios, migrations
```

## Matriz de dependencias

| Proyecto       | Referencia a                          |
| -------------- | ------------------------------------- |
| Domain         | *(ninguno)*                           |
| Application    | Domain                                |
| Infrastructure | Application, Domain                   |
| Persistence    | Application, Domain                   |
| Api            | Application, Infrastructure, Persistence |

> Regla de Clean Architecture: las dependencias apuntan **hacia adentro**.
> Application define puertos (interfaces) → Persistence/Infrastructure los implementan.

## Compilar y ejecutar

```powershell
# Desde la raíz del repo
.\scripts\utils\activate-env.ps1
cd backend
dotnet restore
dotnet build
dotnet run --project src\FutbolManager.Api
```

API disponible en `https://localhost:5001` con Swagger en `/swagger`.

## Convenciones

* Documentación XML obligatoria en todos los miembros públicos (`<summary>`, `<param>`, `<returns>`).
* Nullable habilitado.
* `TreatWarningsAsErrors = true`.
* Métodos asíncronos con sufijo `Async`.
* Commands/Queries CQRS: `{Verbo}{Sustantivo}{Command|Query}` con su `Handler` y `Validator`.
* Endpoints REST en `kebab-case` y versionados: `/api/v1/equipos`.

> **Estado:** estructura base lista. Sin lógica de negocio implementada.
> Ver `ARQUITECTURA.md` en la raíz del repositorio para el diseño completo.
