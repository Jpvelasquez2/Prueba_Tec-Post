# FutbolManager.Infrastructure

**Servicios técnicos y adaptadores externos.** Implementa puertos de Application que NO sean de base de datos.

## Responsabilidad

* **Autenticación**: emisión y validación de JWT (`IJwtTokenService`).
* **Servicios externos**: email (SMTP/SendGrid), almacenamiento de archivos (filesystem/S3), proveedores HTTP de terceros.
* **Sistema**: `IDateTimeProvider`, `ICurrentUserService`.
* **Logging estructurado** (Serilog) y enriquecedores (CorrelationId).
* **Caché** distribuida (Redis/MemoryCache).

## Restricciones

* No contiene acceso a la base de datos (eso es responsabilidad de Persistence).
* No contiene lógica de negocio: solo cumple contratos definidos en Application.
* Cada integración externa debe estar detrás de una interfaz que viva en Application.

## Estructura prevista

```
FutbolManager.Infrastructure/
├── Authentication/        # JwtTokenService, CurrentUserService
├── Services/              # DateTimeProvider, EmailService, FileStorageService
├── Logging/
├── Caching/
└── ExternalApis/
```
