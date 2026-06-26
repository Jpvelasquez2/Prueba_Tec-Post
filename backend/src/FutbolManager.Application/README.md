# FutbolManager.Application

**Casos de uso del sistema.** Orquesta el dominio. Solo depende de Domain.

## Responsabilidad

* **CQRS con MediatR**: Commands (mutaciones) y Queries (lecturas) por feature.
* **Handlers** que ejecutan los casos de uso usando entidades del dominio.
* **DTOs** de entrada/salida (`*Request`, `*Response`, `*Dto`).
* **Validadores** con FluentValidation.
* **Mappings** (AutoMapper o Mapster).
* **Interfaces (puertos)**: `IEquipoRepository`, `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUserService`, etc.
  Las implementa Persistence o Infrastructure.
* **Behaviors / Pipelines** transversales: validación, logging, transacciones, caché.

## Restricciones

* No accede a EF Core ni a HTTP directamente.
* No referencia a Infrastructure ni a Persistence (Dependency Inversion).
* Define la forma de los contratos; las capas externas los cumplen.

## Estructura prevista

```
FutbolManager.Application/
├── Common/
│   ├── Behaviors/         # ValidationBehavior, LoggingBehavior, TransactionBehavior
│   ├── Exceptions/        # NotFoundException, ValidationException
│   ├── Interfaces/        # Puertos hacia Infrastructure/Persistence
│   ├── Mappings/
│   ├── Models/            # Result<T>, PagedResult<T>
│   └── Security/
└── Features/
    ├── Equipos/{Commands,Queries,Dtos}
    ├── Jugadores/
    ├── Partidos/
    └── Torneos/
```
