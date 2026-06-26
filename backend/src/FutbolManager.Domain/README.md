# FutbolManager.Domain

**Núcleo del negocio.** No depende de ninguna otra capa ni de librerías de infraestructura.

## Responsabilidad

* **Entidades y agregados** (Equipo, Jugador, Partido, Torneo) con comportamiento (no anémicas).
* **Value Objects** (Marcador, NombreEquipo, Estadio).
* **Enums** del dominio (EstadoPartido, Posicion).
* **Eventos de dominio** (PartidoFinalizadoEvent).
* **Excepciones de dominio** (BusinessRuleException).
* **Especificaciones** (patrón Specification para queries de dominio).

## Restricciones

* **Cero dependencias** sobre EF Core, ASP.NET, MediatR, Newtonsoft, etc.
* Toda la lógica de invariantes vive aquí.
* Si una regla puede expresarse sin tocar la base de datos ni el HTTP, debe vivir en Domain.

## Estructura prevista

```
FutbolManager.Domain/
├── Common/        # Entity, AggregateRoot, ValueObject, IDomainEvent
├── Entities/      # Equipo, Jugador, Partido, Torneo
├── ValueObjects/
├── Enums/
├── Events/
├── Exceptions/
└── Specifications/
```
