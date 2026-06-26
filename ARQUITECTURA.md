# Arquitectura — Gestión de Equipos y Partidos de Fútbol

> Documento de diseño arquitectónico previo a la implementación.

---

## 1. Visión general

Solución **full-stack** dividida en dos productos desplegables independientes:

| Producto      | Stack                                                              | Responsabilidad                                                     |
| ------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------- |
| **Frontend**  | Angular (LTS) + TypeScript + Tailwind CSS + RxJS                   | SPA que consume la API REST. Arquitectura Feature-Based.            |
| **Backend**   | ASP.NET Core (.NET LTS) + EF Core + SQL Server                     | API REST en Clean Architecture (API/Application/Domain/Infrastructure/Persistence). |

**Principios rectores:** Clean Architecture, SOLID, DDD ligero, CQRS con MediatR, Separation of Concerns, Dependency Inversion, código testeable.

```
┌────────────────────────┐        HTTPS / JSON         ┌──────────────────────────┐         ┌─────────────┐
│  Angular SPA           │  ─────────────────────────▶ │  ASP.NET Core Web API    │  ─────▶ │ SQL Server  │
│  (Feature-Based)       │  ◀───────────────────────── │  (Clean Architecture)    │  ◀───── │             │
└────────────────────────┘                             └──────────────────────────┘         └─────────────┘
```

---

## 2. Estructura de la solución (raíz)

```
Prueba_Tec-Post/
├── README.md
├── ARQUITECTURA.md
├── docker-compose.yml                 # Orquestación opcional (API + SQL Server)
├── .editorconfig
├── .gitignore
│
├── backend/
│   ├── FutbolManager.sln
│   ├── src/
│   │   ├── FutbolManager.Api/
│   │   ├── FutbolManager.Application/
│   │   ├── FutbolManager.Domain/
│   │   ├── FutbolManager.Infrastructure/
│   │   └── FutbolManager.Persistence/
│   └── tests/
│       ├── FutbolManager.Domain.UnitTests/
│       ├── FutbolManager.Application.UnitTests/
│       ├── FutbolManager.Infrastructure.IntegrationTests/
│       └── FutbolManager.Api.FunctionalTests/
│
└── frontend/
    └── futbol-manager-web/            # Workspace Angular
```

---

## 3. Backend — Clean Architecture

### 3.1 Diagrama de dependencias

```
              ┌───────────────────────────┐
              │           Api             │  (Presentation)
              │  Controllers / Filters /  │
              │  Middlewares / DI Root    │
              └───────────────┬───────────┘
                              │ depende de
              ┌───────────────▼───────────┐         ┌─────────────────────────┐
              │       Application         │ ◀────── │     Infrastructure      │
              │ Use Cases / CQRS /        │         │  Servicios externos,    │
              │ Validators / DTOs /       │         │  Logging, Email, JWT,   │
              │ Interfaces (puertos)      │         │  Cache, Storage         │
              └───────────────┬───────────┘         └────────────┬────────────┘
                              │                                  │
                              │                                  │
              ┌───────────────▼──────────────────────────────────▼──┐
              │                    Persistence                       │
              │  DbContext, Migrations, Repositorios EF Core,        │
              │  Configuraciones Fluent API, UnitOfWork              │
              └───────────────────────┬──────────────────────────────┘
                                      │
                                      ▼
                          ┌──────────────────────┐
                          │       Domain         │  (Núcleo, sin dependencias)
                          │ Entities, ValueObjs, │
                          │ Enums, Reglas,       │
                          │ Domain Events,       │
                          │ Excepciones de       │
                          │ Dominio              │
                          └──────────────────────┘
```

las dependencias siempre apuntan hacia adentro. **Domain no depende de nada.** Infrastructure y Persistence dependen de Application/Domain mediante interfaces (Dependency Inversion). Api es el *composition root*.

### 3.2 Responsabilidades por capa

#### 3.2.1 `Domain` — El núcleo del negocio

* Entidades ricas (Equipo, Jugador, Partido, Torneo, etc.) con comportamiento, no anémicas.
* **Value Objects** (`Marcador`, `NombreEquipo`, `Estadio`).
* **Enums** del dominio (`EstadoPartido`, `Posicion`).
* **Eventos de dominio** (`PartidoFinalizadoEvent`).
* **Excepciones de dominio** (`ReglaDeNegocioException`).
* Interfaces de **especificaciones** y agregados.
* Sin referencias a EF Core, ASP.NET, ni librerías de infraestructura.

#### 3.2.2 `Application` — Casos de uso

* **CQRS con MediatR**: `Commands` (mutaciones) y `Queries` (lecturas) por feature.
* **Handlers** que orquestan el dominio.
* **DTOs** de entrada/salida.
* **Validadores con FluentValidation**.
* **Mapeos con AutoMapper** (o Mapster).
* **Interfaces (puertos)** que serán implementadas por Infrastructure/Persistence: `IEquipoRepository`, `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUserService`, `INotificationService`.
* **Behaviors / Pipelines** transversales: validación, logging, transacciones, manejo de excepciones, caché de queries.
* Sin acceso directo a EF Core ni a HTTP.

#### 3.2.3 `Infrastructure` — Servicios técnicos

* Implementaciones de servicios externos: email, almacenamiento, JWT, caché distribuida, integraciones HTTP a terceros.
* Adaptadores para `IDateTimeProvider`, `IEmailService`, `IFileStorage`, `ITokenService`.
* Configuración de logging (Serilog) y métricas.
* No contiene acceso a la base de datos (eso vive en Persistence).

#### 3.2.4 `Persistence` — Acceso a datos

* `ApplicationDbContext` (EF Core).
* **Configuraciones Fluent API** por entidad (`EquipoConfiguration`, `PartidoConfiguration`).
* **Migrations**.
* Implementaciones de repositorios definidos en Application.
* `UnitOfWork` que envuelve `SaveChangesAsync` y publica eventos de dominio.
* **Seeders** para datos iniciales (equipos demo, torneo demo).
* Interceptors EF (auditoría: CreatedAt/UpdatedAt, soft delete).

#### 3.2.5 `Api` — Presentación

* Controllers REST versionados (`/api/v1/...`).
* **Minimal API** o Controllers — recomendado Controllers por trazabilidad y Swagger.
* Middlewares: manejo global de excepciones, correlación, autenticación JWT, CORS, rate limiting.
* Filtros: validación de modelo, autorización.
* Configuración Swagger/OpenAPI.
* DI Composition Root: registra Application, Infrastructure, Persistence.
* No contiene lógica de negocio.

### 3.3 Estructura de carpetas detallada del backend

```
backend/src/

├── FutbolManager.Api/
│   ├── Controllers/
│   │   └── V1/
│   │       ├── EquiposController.cs
│   │       ├── JugadoresController.cs
│   │       ├── PartidosController.cs
│   │       └── TorneosController.cs
│   ├── Middlewares/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── CorrelationIdMiddleware.cs
│   ├── Filters/
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs        # AddApiServices()
│   │   └── WebApplicationExtensions.cs           # UseApiPipeline()
│   ├── Configuration/
│   │   ├── SwaggerConfig.cs
│   │   ├── CorsConfig.cs
│   │   └── JwtConfig.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── FutbolManager.Api.csproj
│
├── FutbolManager.Application/
│   ├── Common/
│   │   ├── Behaviors/                            # ValidationBehavior, LoggingBehavior, TransactionBehavior
│   │   ├── Exceptions/                           # NotFoundException, ValidationException, ConflictException
│   │   ├── Interfaces/                           # IApplicationDbContext, IUnitOfWork, IDateTimeProvider, ICurrentUserService
│   │   ├── Mappings/                             # AutoMapper Profiles base
│   │   ├── Models/                               # Result<T>, PagedResult<T>, PaginationParams
│   │   └── Security/                             # IAuthorizationPolicy
│   ├── Features/
│   │   ├── Equipos/
│   │   │   ├── Commands/
│   │   │   │   ├── CrearEquipo/
│   │   │   │   ├── ActualizarEquipo/
│   │   │   │   └── EliminarEquipo/
│   │   │   ├── Queries/
│   │   │   │   ├── ObtenerEquipoPorId/
│   │   │   │   └── ListarEquipos/
│   │   │   └── Dtos/
│   │   ├── Jugadores/
│   │   ├── Partidos/
│   │   │   ├── Commands/
│   │   │   │   ├── ProgramarPartido/
│   │   │   │   ├── RegistrarResultado/
│   │   │   │   └── CancelarPartido/
│   │   │   ├── Queries/
│   │   │   └── EventHandlers/                    # Reacciona a PartidoFinalizadoEvent
│   │   └── Torneos/
│   ├── DependencyInjection.cs                    # AddApplicationServices()
│   └── FutbolManager.Application.csproj
│
├── FutbolManager.Domain/
│   ├── Common/
│   │   ├── Entity.cs                             # Base abstract
│   │   ├── AggregateRoot.cs
│   │   ├── ValueObject.cs
│   │   ├── IDomainEvent.cs
│   │   └── BusinessRuleException.cs
│   ├── Entities/
│   │   ├── Equipo.cs
│   │   ├── Jugador.cs
│   │   ├── Partido.cs
│   │   └── Torneo.cs
│   ├── ValueObjects/
│   │   ├── Marcador.cs
│   │   └── NombreEquipo.cs
│   ├── Enums/
│   │   ├── EstadoPartido.cs
│   │   └── Posicion.cs
│   ├── Events/
│   │   └── PartidoFinalizadoEvent.cs
│   ├── Exceptions/
│   ├── Specifications/
│   └── FutbolManager.Domain.csproj
│
├── FutbolManager.Infrastructure/
│   ├── Authentication/
│   │   ├── JwtTokenService.cs
│   │   └── CurrentUserService.cs
│   ├── Services/
│   │   ├── DateTimeProvider.cs
│   │   ├── EmailService.cs
│   │   └── FileStorageService.cs
│   ├── Logging/
│   ├── Caching/
│   ├── ExternalApis/
│   ├── DependencyInjection.cs                    # AddInfrastructureServices()
│   └── FutbolManager.Infrastructure.csproj
│
└── FutbolManager.Persistence/
    ├── Contexts/
    │   └── ApplicationDbContext.cs
    ├── Configurations/                           # IEntityTypeConfiguration<>
    │   ├── EquipoConfiguration.cs
    │   ├── JugadorConfiguration.cs
    │   ├── PartidoConfiguration.cs
    │   └── TorneoConfiguration.cs
    ├── Repositories/
    │   ├── EquipoRepository.cs
    │   ├── JugadorRepository.cs
    │   └── PartidoRepository.cs
    ├── UnitOfWork/
    │   └── UnitOfWork.cs
    ├── Interceptors/                             # Auditing, SoftDelete, DomainEventDispatcher
    ├── Migrations/
    ├── Seed/
    │   └── DatabaseSeeder.cs
    ├── DependencyInjection.cs                    # AddPersistenceServices()
    └── FutbolManager.Persistence.csproj
```

### 3.4 Matriz de dependencias entre proyectos

| Proyecto        | Referencia a                                          |
| --------------- | ----------------------------------------------------- |
| Domain          | *(ninguno)*                                           |
| Application     | Domain                                                |
| Infrastructure  | Application, Domain                                   |
| Persistence     | Application, Domain                                   |
| Api             | Application, Infrastructure, Persistence              |

Application **nunca** referencia a Infrastructure/Persistence: las usa por **interfaces** que ellas implementan.

---

## 4. Frontend — Arquitectura Feature-Based (Angular)

### 4.1 Principios

* **Standalone components** y **signals** (Angular moderno).
* Separación en tres anillos: **Core / Shared / Features**.
* **Lazy loading** por feature.
* **Smart vs Dumb components**: contenedores con estado, presentacionales puros.
* Estado local con **signals**; comunicación reactiva con **RxJS**.
* Estado global ligero con un store por feature (signal-based) o NgRx si crece.
* **Tailwind CSS** para estilos utilitarios; tokens de diseño centralizados.
* **HTTP Interceptors** para auth, errores y correlación.

### 4.2 Capas conceptuales

```
┌───────────────────────────────────────────────────────────┐
│                         App Shell                          │
│  Routing raíz · Layout · Guards globales                   │
└───────────────────────────┬───────────────────────────────┘
                            │
   ┌────────────────────────┼─────────────────────────┐
   ▼                        ▼                         ▼
┌────────┐            ┌────────────┐            ┌──────────┐
│  Core  │            │  Features  │            │  Shared  │
│        │            │            │            │          │
│ Servicios │         │ equipos/    │           │ UI atoms │
│ singleton │         │ jugadores/  │           │ pipes    │
│ Guards    │         │ partidos/   │           │ directives│
│ Interceptors│       │ torneos/    │           │ utils    │
│ Modelos base│       │ dashboard/  │           │          │
└────────┘            └────────────┘            └──────────┘
```

### 4.3 Estructura de carpetas detallada del frontend

```
frontend/futbol-manager-web/
├── angular.json
├── package.json
├── tailwind.config.js
├── postcss.config.js
├── tsconfig.json
├── tsconfig.app.json
├── .eslintrc.json
├── .prettierrc
└── src/
    ├── main.ts
    ├── index.html
    ├── styles.css                          # Tailwind directives + tokens
    │
    ├── app/
    │   ├── app.component.ts
    │   ├── app.routes.ts
    │   ├── app.config.ts                   # providers, interceptors, router
    │   │
    │   ├── core/
    │   │   ├── auth/
    │   │   │   ├── auth.service.ts
    │   │   │   ├── auth.guard.ts
    │   │   │   ├── auth.interceptor.ts
    │   │   │   └── auth.models.ts
    │   │   ├── http/
    │   │   │   ├── api-base.service.ts
    │   │   │   ├── error.interceptor.ts
    │   │   │   └── correlation.interceptor.ts
    │   │   ├── config/
    │   │   │   └── app.config.token.ts
    │   │   ├── models/                     # Result, Paginated, ApiError
    │   │   └── layout/
    │   │       ├── main-layout/
    │   │       ├── header/
    │   │       └── sidebar/
    │   │
    │   ├── shared/
    │   │   ├── ui/                         # Botones, inputs, modales, cards
    │   │   │   ├── button/
    │   │   │   ├── input/
    │   │   │   ├── modal/
    │   │   │   └── table/
    │   │   ├── pipes/
    │   │   ├── directives/
    │   │   ├── validators/
    │   │   └── utils/
    │   │
    │   └── features/
    │       ├── equipos/
    │       │   ├── equipos.routes.ts
    │       │   ├── pages/                  # Smart components (rutas)
    │       │   │   ├── equipos-list/
    │       │   │   ├── equipo-detail/
    │       │   │   └── equipo-form/
    │       │   ├── components/             # Dumb components del feature
    │       │   │   ├── equipo-card/
    │       │   │   └── equipo-roster/
    │       │   ├── data-access/
    │       │   │   ├── equipos.service.ts  # HTTP
    │       │   │   ├── equipos.store.ts    # Signal store
    │       │   │   └── equipos.mapper.ts
    │       │   ├── models/
    │       │   │   ├── equipo.model.ts
    │       │   │   └── equipo.dto.ts
    │       │   └── guards/
    │       │
    │       ├── jugadores/
    │       ├── partidos/
    │       │   ├── partidos.routes.ts
    │       │   ├── pages/
    │       │   │   ├── partidos-calendario/
    │       │   │   ├── partido-detail/
    │       │   │   └── registrar-resultado/
    │       │   ├── components/
    │       │   ├── data-access/
    │       │   └── models/
    │       ├── torneos/
    │       └── dashboard/
    │
    └── environments/
        ├── environment.ts
        └── environment.production.ts
```

### 4.4 Reglas de dependencia en el frontend

| Capa     | Puede importar de                |
| -------- | -------------------------------- |
| Core     | *(nada del dominio del negocio)* |
| Shared   | *(nada)*                         |
| Features | Core, Shared, *(su propia carpeta)* |
| Feature A| **NUNCA** Feature B directamente; comunicación vía router o servicio en Core |

---

## 5. Flujo end-to-end de una petición

**Ejemplo:** el usuario registra el resultado de un partido.

```
[1] Usuario en Angular
    │  Click "Guardar" en <registrar-resultado>
    ▼
[2] Smart Component (page)
    │  Toma valores de un Reactive Form, llama a partidos.store.registrarResultado()
    ▼
[3] Signal Store / Service (data-access)
    │  Convierte el modelo de vista en RegistrarResultadoRequestDto (mapper)
    │  Llama a partidos.service.registrarResultado(dto)
    ▼
[4] HTTP Service (RxJS)
    │  POST /api/v1/partidos/{id}/resultado
    │  AuthInterceptor agrega Bearer token
    │  CorrelationInterceptor agrega X-Correlation-Id
    ▼
[5] Network → ASP.NET Core
    │  CORS valida origen
    │  Middleware: CorrelationId, ExceptionHandling, Authentication, Authorization
    ▼
[6] PartidosController (Api)
    │  Mapea body → RegistrarResultadoCommand
    │  Envía: await _mediator.Send(command)
    ▼
[7] MediatR Pipeline (Application)
    │  ValidationBehavior (FluentValidation)
    │  LoggingBehavior
    │  TransactionBehavior (abre transacción)
    ▼
[8] RegistrarResultadoCommandHandler (Application)
    │  partido = await _partidoRepo.GetByIdAsync(id)   ── usa IPartidoRepository (puerto)
    │  partido.RegistrarResultado(marcadorLocal, marcadorVisitante)  ── reglas de Domain
    │  await _unitOfWork.SaveChangesAsync()
    ▼
[9] Domain (Partido entity)
    │  Valida invariantes (no doble registro, estado correcto…)
    │  Levanta PartidoFinalizadoEvent
    ▼
[10] Persistence
     │  EF Core traduce a SQL
     │  Interceptor de auditoría rellena UpdatedAt/UpdatedBy
     │  Interceptor de eventos publica PartidoFinalizadoEvent vía MediatR tras commit
     ▼
[11] SQL Server
     │  UPDATE Partidos SET ... ; INSERT INTO OutboxMessages (opcional)
     ▼
[12] Vuelta arriba
     │  Handler devuelve Result<PartidoDto>
     │  Controller responde 200 OK + body
     │  Interceptor del front captura respuesta, store actualiza signal
     │  La vista se re-renderiza con el nuevo marcador
```

### 5.1 Manejo de errores

* **Dominio** lanza excepciones específicas (`BusinessRuleException`).
* **Application** lanza `NotFoundException`, `ValidationException`, `ConflictException`.
* **Api** las captura en `ExceptionHandlingMiddleware` y devuelve **ProblemDetails** RFC 7807 con `traceId`.
* **Frontend** centraliza en `error.interceptor.ts` → notificación al usuario + log.

---

## 6. Convenciones de nombres

### 6.1 Backend (C#)

| Elemento                        | Convención                                       | Ejemplo                                  |
| ------------------------------- | ------------------------------------------------ | ---------------------------------------- |
| Proyectos                       | `Empresa.Producto.Capa`                          | `FutbolManager.Application`              |
| Namespaces                      | Reflejan la carpeta                              | `FutbolManager.Application.Features.Equipos.Commands.CrearEquipo` |
| Clases / records / interfaces   | `PascalCase`; interfaces con prefijo `I`         | `EquipoRepository`, `IEquipoRepository`  |
| Métodos / propiedades públicas  | `PascalCase`                                     | `RegistrarResultado()`                   |
| Campos privados                 | `_camelCase`                                     | `_dbContext`                             |
| Constantes                      | `PascalCase`                                     | `MaxJugadoresPorEquipo`                  |
| Async                           | Sufijo `Async`                                   | `GetByIdAsync`                           |
| Commands / Queries (CQRS)       | `Verbo + Sustantivo + Command|Query`             | `CrearEquipoCommand`, `ListarEquiposQuery` |
| Handlers                        | `{Command|Query}Handler`                         | `CrearEquipoCommandHandler`              |
| DTOs                            | Sufijo `Dto`, `Request`, `Response`              | `EquipoDto`, `CrearEquipoRequest`        |
| Validadores                     | `{Comando}Validator`                             | `CrearEquipoCommandValidator`            |
| Configuraciones EF              | `{Entidad}Configuration`                         | `PartidoConfiguration`                   |
| Excepciones                     | Sufijo `Exception`                               | `BusinessRuleException`                  |
| Endpoints                       | Plural en `kebab-case`, versión                  | `GET /api/v1/equipos/{id}/jugadores`     |

### 6.2 Frontend (Angular / TS)

| Elemento                   | Convención                                | Ejemplo                          |
| -------------------------- | ----------------------------------------- | -------------------------------- |
| Archivos                   | `kebab-case` + sufijo de tipo             | `equipo-card.component.ts`       |
| Componentes                | `PascalCase` + sufijo `Component`         | `EquipoCardComponent`            |
| Servicios                  | Sufijo `Service`                          | `EquiposService`                 |
| Stores                     | Sufijo `Store`                            | `EquiposStore`                   |
| Modelos                    | Sufijo según rol: `.model.ts`, `.dto.ts`  | `equipo.model.ts`                |
| Rutas de feature           | `<feature>.routes.ts`                     | `equipos.routes.ts`              |
| Guards / Interceptores     | Sufijo `.guard.ts`, `.interceptor.ts`     | `auth.guard.ts`                  |
| Pipes / Directivas         | Sufijo `.pipe.ts`, `.directive.ts`        | `time-ago.pipe.ts`               |
| Selectores HTML            | Prefijo `app-`                            | `<app-equipo-card>`              |
| Variables / funciones      | `camelCase`                               | `cargarEquipos()`                |
| Tipos / interfaces         | `PascalCase` sin prefijo `I`              | `Equipo`, `Partido`              |
| Constantes globales        | `SCREAMING_SNAKE_CASE`                    | `API_BASE_URL`                   |
| Tests                      | `*.spec.ts`                               | `equipos.service.spec.ts`        |

### 6.3 Base de datos

* Tablas en **PascalCase** y plural (`Equipos`, `Partidos`).
* PK: `Id`. FK: `{Entidad}Id` (`EquipoLocalId`, `EquipoVisitanteId`).
* Auditoría: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`.
* Migraciones: `yyyyMMdd_DescripcionEnPascalCase`.

---

## 7. Buenas prácticas aplicadas

### 7.1 SOLID

| Principio | Aplicación concreta |
| --------- | ------------------- |
| **S — Single Responsibility** | Cada handler resuelve **un** caso de uso; cada componente Angular tiene un propósito. |
| **O — Open/Closed**           | Nuevos features se agregan creando carpetas/handlers, sin tocar código existente. Pipeline de MediatR extensible con behaviors. |
| **L — Liskov**                | Implementaciones de repositorios y servicios son intercambiables; los tests usan dobles que cumplen el contrato. |
| **I — Interface Segregation** | `IEquipoRepository`, `IPartidoRepository` separados, no un `IGenericRepository<T>` con métodos no usados. |
| **D — Dependency Inversion**  | Application define las interfaces; Persistence/Infrastructure las implementan; Api las cablea por DI. |

### 7.2 Clean Code

* Nombres expresivos en español del dominio (`RegistrarResultado`, `ProgramarPartido`).
* Funciones cortas, una sola responsabilidad.
* Sin comentarios obvios; comentar solo el **por qué**.
* Evitar primitivos obsesivos → Value Objects (`Marcador`, `NombreEquipo`).
* Entidades **ricas**, no estructuras de datos planas.
* Inmutabilidad cuando aplica (`record` para DTOs, signals/readonly en Angular).
* Manejo explícito de errores con `Result<T>` o excepciones de dominio bien tipadas.

### 7.3 Separation of Concerns

* Capas con responsabilidad única (UI, casos de uso, reglas, datos, técnico).
* Features autocontenidos en el frontend (rutas, vistas, modelos, servicios).
* Mapeo explícito DTO ↔ Entidad ↔ ViewModel — nunca exponer entidades de dominio en la API.

### 7.4 Calidad y operación

* **Validación**: FluentValidation en Application; Reactive Forms + validators tipados en Angular.
* **Logs estructurados**: Serilog con enriquecedores (CorrelationId, UserId).
* **Trazabilidad**: `X-Correlation-Id` end-to-end.
* **Seguridad**: JWT, autorización por políticas, CORS estricto, validación de entrada, HTTPS, headers (HSTS, CSP), rate limiting.
* **OpenAPI/Swagger** con anotaciones y ejemplos.
* **Versionado de API** desde el día 1 (`/api/v1`).
* **Migrations** versionadas y reproducibles.
* **Health checks** (`/health/live`, `/health/ready`).
* **Tests**: unitarios (Domain, Application), integración (Persistence con SQL Server in-memory o Testcontainers), funcionales (Api con `WebApplicationFactory`), E2E (Angular con Playwright/Cypress opcional).
* **CI/CD**: pipeline con build, lint, test, análisis (SonarCloud opcional), publish.
* **Configuración por entorno** vía `appsettings.{Env}.json` + variables de entorno; nunca secretos en repo.

### 7.5 Preparación para crecimiento futuro

* **CQRS** facilita escalar lecturas (proyecciones, caché, vistas materializadas).
* **Eventos de dominio** + patrón **Outbox** (placeholder en Persistence) permiten integrarse con mensajería (RabbitMQ, Azure Service Bus) sin reescribir.
* **Feature-based** en Angular permite extraer módulos a micro-frontends si crece.
* **Persistence aislado** permite cambiar el ORM o agregar read-models en otra base.
* **Infrastructure** desacopla servicios técnicos: mañana cambiar de SendGrid a SES es tocar una sola clase.
* **Multi-tenancy** futuro: basta agregar `TenantId` en `Entity` base + filtros globales en `DbContext`.

---

## 8. Resumen

* **Backend**: 5 proyectos (Api · Application · Domain · Infrastructure · Persistence) bajo Clean Architecture con CQRS, MediatR y EF Core sobre SQL Server.
* **Frontend**: Angular standalone con arquitectura Feature-Based (Core/Shared/Features), lazy loading, signals + RxJS, Tailwind.
* **Comunicación**: REST JSON versionada, autenticación JWT, manejo uniforme de errores (ProblemDetails), correlación end-to-end.
* **Calidad**: SOLID, Clean Code, separación estricta de capas, tests por nivel, observabilidad integrada.
* **Listo para escalar**: CQRS, eventos de dominio, outbox-ready, features lazy, infraestructura sustituible.

