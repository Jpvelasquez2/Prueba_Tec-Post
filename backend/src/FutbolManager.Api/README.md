# FutbolManager.Api

**Capa de presentación y Composition Root.** Expone la API REST y cablea todas las dependencias.

## Responsabilidad

* **Controllers REST versionados** (`/api/v1/...`).
* **Middlewares**: manejo global de excepciones (ProblemDetails RFC 7807), correlación, autenticación.
* **Filtros**: validación de modelo, autorización por políticas.
* **Configuración** Swagger/OpenAPI con anotaciones y ejemplos.
* **Composition Root**: registra los servicios de Application, Infrastructure y Persistence.
* CORS y rate limiting.

## Restricciones

* **No contiene lógica de negocio** — delega todo en `IMediator.Send(...)`.
* Los DTOs de Application son los contratos públicos; nunca se expone una entidad de dominio.
* Cualquier cambio de regla viene desde Domain/Application.

## Estructura prevista

```
FutbolManager.Api/
├── Controllers/V1/        # EquiposController, PartidosController, ...
├── Middlewares/           # ExceptionHandling, CorrelationId
├── Filters/
├── Extensions/            # AddApiServices, UseApiPipeline
├── Configuration/         # SwaggerConfig, CorsConfig, JwtConfig
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

## Ejecutar

```powershell
dotnet run --project src\FutbolManager.Api
```

Swagger UI: `https://localhost:5001/swagger`.
