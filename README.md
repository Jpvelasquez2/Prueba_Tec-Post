# FutbolManager — Gestión de Equipos y Partidos de Fútbol

[![.NET](https://img.shields.io/badge/.NET-8.0_LTS-512BD4)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-22_LTS-DD0031)](https://angular.dev/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019%2B-CC2927)](https://www.microsoft.com/sql-server)
[![Tests](https://img.shields.io/badge/tests-54%2F54-success)](#ejecución-de-tests)
[![Coverage](https://img.shields.io/badge/calc__posiciones-100%25-brightgreen)](#ejecución-de-tests)

Solución **full-stack** para gestionar equipos, programar partidos, registrar
resultados y consultar la tabla de posiciones de un torneo de fútbol.

---

## 1. Descripción del proyecto

`FutbolManager` permite a un administrador:

* **Equipos** — crear, editar, consultar y eliminar equipos (con nombre único,
  ciudad y escudo opcionales).
* **Partidos** — programar enfrentamientos (no permite que un equipo juegue
  contra sí mismo), editarlos, eliminarlos, registrar el resultado final y
  cambiar su estado.
* **Tabla de posiciones** — consultar el ranking calculado automáticamente
  con `PJ · PG · PE · PP · GF · GC · DG · PTS` y ordenamiento por
  `PTS → DG → GF → Nombre`.
* **Filtros** — por equipo, estado y fecha en el listado de partidos; por
  nombre de equipo en la tabla de posiciones.

> Toda regla de negocio vive en el dominio (no en controladores ni en SQL);
> el frontend es un cliente delgado sobre la API REST.

---

## 2. Arquitectura

### 2.1 Vista general

```
┌────────────────────────┐  HTTPS / JSON  ┌──────────────────────────┐         ┌─────────────┐
│  Angular SPA           │  ────────────▶ │  ASP.NET Core Web API    │  ─────▶ │ SQL Server  │
│  (Feature-Based)       │  ◀──────────── │  (Clean Architecture)    │  ◀───── │             │
└────────────────────────┘                └──────────────────────────┘         └─────────────┘
```

### 2.2 Backend — Clean Architecture

```
              ┌─────────────────────────────┐
              │            Api              │  Presentation / Composition Root
              │  Controllers · Middlewares  │
              │  Swagger · DI registration  │
              └─────────────────┬───────────┘
                                │
              ┌─────────────────▼───────────┐         ┌─────────────────────────┐
              │       Application           │  ◀───── │     Infrastructure      │
              │  Services (CRUD) · DTOs     │         │  Servicios técnicos     │
              │  AutoMapper · Validadores   │         │  (espacio reservado     │
              │  Interfaces (puertos)       │         │   para JWT, email…)     │
              └─────────────────┬───────────┘         └────────────┬────────────┘
                                │                                  │
              ┌─────────────────▼──────────────────────────────────▼──┐
              │                  Persistence                            │
              │  ApplicationDbContext · Configurations · Repositorios   │
              │  UnitOfWork · (Espacio para migrations/interceptors)    │
              └────────────────────────┬────────────────────────────────┘
                                       │
                                       ▼
                          ┌──────────────────────┐
                          │       Domain         │  Núcleo (sin dependencias)
                          │  Entidades ricas     │
                          │  Enums · Excepciones │
                          │  Posiciones (puro)   │
                          └──────────────────────┘
```

**Regla de oro:** las dependencias apuntan hacia adentro. `Domain` no depende
de nada. `Application` define interfaces que `Persistence` e `Infrastructure`
implementan (Dependency Inversion). `Api` es el *composition root*.

### 2.3 Frontend — Feature-Based + Signals

```
src/app/
├── core/        ── singletons: http (interceptores), ui (toast, confirm, loader),
│                   layout (header + navegación), modelos base
├── shared/      ── ui presentacional reutilizable (form-error, empty-state)
└── features/
    ├── equipos/      (lazy)
    ├── partidos/     (lazy)
    └── posiciones/   (lazy)
```

Cada feature contiene **modelos**, **data-access** (HttpClient), **pages**
(componentes smart) y sus **routes**. Las features **no se importan entre sí**;
se comunican solo vía router.

> Detalle completo de capas, flujo de una petición y convenciones en
> [`ARQUITECTURA.md`](ARQUITECTURA.md).

---

## 3. Tecnologías

| Capa | Stack |
|---|---|
| **Backend** | ASP.NET Core 8 LTS · EF Core 8 · SQL Server · AutoMapper 13 · Swashbuckle 6 |
| **Frontend** | Angular 22 LTS · TypeScript · Tailwind CSS 4 · RxJS · Signals (zoneless) |
| **Tests** | xUnit 2.9 · Moq 4.20 · FluentAssertions 6.12 · Coverlet |
| **Base de datos** | SQL Server (LocalDB o Express en dev; SQL Server en prod) |
| **Tooling portable** | Node.js 24.18 LTS y .NET SDK 8.0.422 en `herramientas/` |

---

## 4. Estructura de carpetas

```
Prueba_Tec-Post/
├── README.md                           # Este archivo
├── ARQUITECTURA.md                     # Diseño arquitectónico extendido
├── .gitignore
│
├── backend/                            # Solución .NET
│   ├── FutbolManager.sln
│   ├── Directory.Build.props           # Props comunes (XML docs, warnings as errors)
│   ├── src/
│   │   ├── FutbolManager.Api/          # Controllers, Middlewares, Program.cs
│   │   ├── FutbolManager.Application/  # Services, DTOs, Interfaces, AutoMapper
│   │   ├── FutbolManager.Domain/       # Entidades, Enums, CalculadorTablaPosiciones
│   │   ├── FutbolManager.Infrastructure/  (espacio reservado)
│   │   └── FutbolManager.Persistence/  # DbContext, Configurations, Repositorios, UoW
│   └── tests/
│       ├── FutbolManager.Domain.UnitTests/        # 49 tests
│       └── FutbolManager.Application.UnitTests/   # 5 tests (con Moq)
│
├── frontend/futbol-manager-web/        # Workspace Angular
│   └── src/app/{core,shared,features}/
│
├── herramientas/                       # SDKs portables (excluidos del repo)
│   ├── nodejs/         (Node 24.18 LTS)
│   └── dotnet-sdk/     (.NET 8.0.422 LTS)
│
└── scripts/
    ├── db/             # 00_create_database, 01_schema, 02_seed, 03_views, 99_drop
    ├── utils/          # activate-env.ps1, setup-tools.ps1
    └── deploy/         # (placeholder)
```

---

## 5. Instalación

### Requisitos previos

* **Windows 10/11** (los scripts son PowerShell; en macOS/Linux usar `bash`).
* **SQL Server** local (LocalDB, Express o Developer Edition) en `localhost` con
  autenticación Windows.
* **Git** (opcional, para clonar).

> No es necesario instalar Node.js ni .NET SDK globalmente: se incluyen
> versiones portables.

### Pasos

1. **Clonar o copiar** el repositorio.

2. **Descargar los SDKs portables** (solo la primera vez):

   ```powershell
   .\scripts\utils\setup-tools.ps1
   ```

   Esto descarga Node.js LTS y .NET SDK LTS dentro de `herramientas/`
   (~830 MB en total, no se versionan).

3. **Activar el entorno portable** en la sesión actual:

   ```powershell
   .\scripts\utils\activate-env.ps1
   ```

   Verificar:

   ```powershell
   dotnet --version   # → 8.0.422
   node --version     # → v24.18.0
   ```

4. **Restaurar paquetes**:

   ```powershell
   cd backend && dotnet restore && cd ..
   cd frontend\futbol-manager-web && npm install && cd ..\..
   ```

---

## 6. Configuración

### Backend — `backend/src/FutbolManager.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=FutbolManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Variables a ajustar:

| Clave | Por defecto | Notas |
|---|---|---|
| `ConnectionStrings:Default` | `Server=localhost;...` | Cambiar `Server` por tu instancia (`localhost\SQLEXPRESS`, etc.). |
| `Logging:LogLevel:Default` | `Information` | A `Debug` para diagnóstico, `Warning` en prod. |

Para producción usar `appsettings.Production.json` o variables de entorno
(`ConnectionStrings__Default=...`).

### Frontend — `frontend/futbol-manager-web/src/environments/environment.ts`

```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5200/api',
} as const;
```

Cambiar `apiUrl` si la API corre en otro host o puerto.

---

## 7. Base de datos

La fuente de verdad del esquema son los scripts en `scripts/db/` (no se usan
migraciones de EF Core, ver [§12](#12-decisiones-de-diseño)).

### Crear desde cero

```powershell
sqlcmd -S localhost -E -i scripts\db\00_create_database.sql
sqlcmd -S localhost -E -d FutbolManagerDb -i scripts\db\01_schema.sql
sqlcmd -S localhost -E -d FutbolManagerDb -i scripts\db\02_seed.sql
sqlcmd -S localhost -E -d FutbolManagerDb -i scripts\db\03_views.sql
```

### Reset (entornos no productivos)

```powershell
sqlcmd -S localhost -E -i scripts\db\99_drop_database.sql
# Repetir los 4 pasos de arriba.
```

### Modelo

* `Equipos` (Id, Nombre UQ, Ciudad?, EscudoUrl?, auditoría)
* `EstadosPartido` (1=Programado, 2=Jugado, 3=Suspendido, 4=Cancelado)
* `Partidos` (Id, Fecha, LocalTeamId FK, VisitanteTeamId FK, LocalScore?,
  VisitanteScore?, EstadoId FK, auditoría)
* `vw_TablaPosiciones` + `sp_ObtenerTablaPosiciones`

**Constraints clave**: `CK_Partidos_EquiposDistintos`,
`CK_Partidos_JugadoTieneResultado`, marcadores ≥ 0, URL del escudo http(s).

Detalle completo (diagrama ER, índices, head-to-head) en
[`scripts/db/README.md`](scripts/db/README.md).

---

## 8. Ejecución del backend

```powershell
.\scripts\utils\activate-env.ps1
cd backend
dotnet run --project src\FutbolManager.Api
```

Por defecto la API arranca en `http://localhost:5200`.

* **Swagger UI**: <http://localhost:5200/swagger>
* **OpenAPI JSON**: <http://localhost:5200/swagger/v1/swagger.json>

Para producción:

```powershell
dotnet publish src\FutbolManager.Api -c Release -o publish\api
```

---

## 9. Ejecución del frontend

```powershell
.\scripts\utils\activate-env.ps1
cd frontend\futbol-manager-web
npm start
```

Abrir <http://localhost:4200>. La home redirige a `/posiciones`.

Build de producción:

```powershell
npm run build
# Salida en dist/futbol-manager-web/
```

---

## 10. Ejecución de tests

```powershell
.\scripts\utils\activate-env.ps1
cd backend
dotnet test --nologo
```

Resultado esperado:

```
Domain.UnitTests:      49 / 49 OK
Application.UnitTests:  5 /  5 OK
─────────────────────────────────
TOTAL:                 54 / 54 OK
```

### Cobertura

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

| Componente | Cobertura |
|---|---|
| **`CalculadorTablaPosiciones`** (objetivo principal) | **100%** |
| `PosicionesService` (con Moq) | 100% |
| Domain (toda la capa) | 89.55% |

Los `cobertura.xml` se generan en `tests/TestResults/`.

---

## 11. Endpoints

Base: `http://localhost:5200/api`

### Equipos — `/teams`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| `GET` | `/teams` | Lista todos los equipos | 200 |
| `GET` | `/teams/{id}` | Obtiene un equipo | 200, 404 |
| `POST` | `/teams` | Crea un equipo | 201, 400, 409 |
| `PUT` | `/teams/{id}` | Actualiza un equipo | 200, 404, 409 |
| `DELETE` | `/teams/{id}` | Elimina un equipo | 204, 404, 409 |

### Partidos — `/matches`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| `GET` | `/matches` | Lista todos los partidos | 200 |
| `GET` | `/matches/{id}` | Obtiene un partido | 200, 404 |
| `POST` | `/matches` | Programa un partido | 201, 400, 404 |
| `PUT` | `/matches/{id}` | Actualiza un partido | 200, 400, 404 |
| `DELETE` | `/matches/{id}` | Elimina un partido | 204, 404 |
| `POST` | `/matches/{id}/result` | Registra resultado (→ Jugado) | 200, 400, 404 |

### Tabla de posiciones — `/standings`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| `GET` | `/standings` | Tabla calculada y ordenada | 200 |

### Modelo de error (`application/problem+json`)

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Recurso no encontrado",
  "status": 404,
  "detail": "No se encontró Partido con id '42'.",
  "traceId": "0HNMJH05N0D68:00000001"
}
```

Mapa de excepciones → HTTP:

| Excepción | HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `BusinessRuleException` | 400 |
| Validación de modelo | 400 (`ValidationProblemDetails`) |
| Cualquier otra | 500 |

---

## 12. Decisiones de diseño

### Backend

1. **Service Pattern + Repository Pattern** sobre EF Core, en vez de CQRS con
   MediatR. La aplicación es pequeña; un handler por caso de uso añadiría
   indirección sin beneficio. Los servicios siguen siendo testeables con Moq.
2. **Entidades ricas** con setters privados y métodos de mutación
   (`Actualizar`, `RegistrarResultado`, `CambiarEstado`). Las invariantes
   viven en el dominio, no en validadores externos.
3. **Catálogo `EstadosPartido` en BD** en vez de `CHECK (Estado IN (...))`.
   Cumple 3FN estrictamente, permite metadatos por estado y es ampliable
   sin alterar tablas hijas.
4. **Calculador puro de posiciones** (`Domain.Posiciones.CalculadorTablaPosiciones`)
   sin dependencias. El servicio en Application solo orquesta. Esto da
   testabilidad trivial (100% cobertura) y permite cambiar el origen de datos
   sin tocar el algoritmo.
5. **Head-to-head documentado y NO implementado**. El cuarto desempate clásico
   requiere recalcular subtotales solo entre equipos empatados; no es
   expresable en un único `ORDER BY`. Se documenta en `scripts/db/README.md`
   y se asegura **orden total** con un desempate alfabético determinístico.
6. **SQL como fuente de verdad** (en vez de EF Migrations). Los scripts en
   `scripts/db/` son idempotentes y reflejan el esquema; EF Core trabaja sobre
   un esquema existente sin generar migraciones. Esto evita drift entre dos
   fuentes de verdad.
7. **`InvariantCultureIgnoreCase` para el tiebreaker alfabético** (descubierto
   por un test): `OrdinalIgnoreCase` ordenaba "Á" después de "Z".
8. **`AddSingleton<CalculadorTablaPosiciones>`** — clase pura y sin estado;
   una instancia compartida es segura y económica.
9. **Global Exception Middleware con `ProblemDetails` (RFC 7807)** + `traceId`
   para correlación con logs.

### Frontend

10. **Angular 22 zoneless** (`provideZonelessChangeDetection`): la app vive
    solo con `signals`, sin Zone.js. Menos overhead y detección predecible.
11. **`toSignal(form.valueChanges)`** para componer filtros con `computed`,
    sin subscripciones manuales que arrastren memory leaks.
12. **Validador cruzado** en el form de partido (`equiposDistintosValidator`)
    reproduce la invariante del dominio; el backend la valida igualmente
    (defensa en profundidad).
13. **Error interceptor centralizado**: los componentes no manejan errores
    HTTP; el interceptor los traduce a toasts con el primer error de campo de
    `ValidationProblemDetails` o el `detail` de `ProblemDetails`.
14. **Carga lazy por feature** + redirección a `/posiciones` por defecto:
    bundle inicial 81 kB transferidos.
15. **Tailwind 4 CSS-first**: sin `tailwind.config.js`; los tokens viven en
    `styles.css`.

---

## 13. Buenas prácticas aplicadas

### Backend

* **SOLID**: SRP en services y handlers, OCP vía interfaces, DIP en toda la
  estructura de Clean Architecture.
* **XML docs obligatorios** (`GenerateDocumentationFile`) en miembros públicos.
* **`TreatWarningsAsErrors = true`** + `Nullable enable`.
* **`async/await` + `CancellationToken`** propagado hasta los repos.
* **`AsNoTracking`** en queries de solo lectura.
* **Índice filtrado** `IX_Partidos_Jugados` para acelerar la vista de
  posiciones (index-only scan).
* **Logging estructurado** vía `ILogger<T>` en cada servicio + niveles
  diferenciados en el middleware (Error 5xx / Warning 4xx).

### Frontend

* **JSDoc** en todas las clases, métodos y propiedades públicas.
* **Standalone components** + signals.
* **Reactive Forms** con `nonNullable` controls y validators tipados.
* **Separación Smart / Dumb**: páginas con estado, componentes
  presentacionales puros (`form-error`, `empty-state`).
* **No se exponen entidades**; siempre DTOs en el modelo del frontend.
* **Confirmación destructiva** previa a `DELETE` (servicio + dialog).
* **Mobile-first responsive** con utilidades Tailwind.

### Calidad

* **Tests unitarios xUnit + Moq + FluentAssertions** con Arrange/Act/Assert.
* **Cobertura 100%** sobre el algoritmo crítico.
* **CI-ready**: `dotnet build` y `npm run build` sin warnings.

---

## 14. Posibles mejoras futuras

### Producto

* **Torneos**: agrupar partidos por torneo o temporada; la tabla de posiciones
  se calcularía por torneo.
* **Jugadores y plantillas**: extender el dominio con jugadores, dorsales,
  altas y bajas.
* **Calendario visual** del feature Partidos (vista mensual / semanal).
* **Estadísticas avanzadas** (forma reciente, racha, máximos goleadores).

### Backend

* **Head-to-head**: implementar el 4º desempate en el orquestador
  (`PosicionesService`), recalculando entre equipos empatados de forma
  recursiva, con sus tests dedicados.
* **CQRS con MediatR + pipelines** (`ValidationBehavior`, `TransactionBehavior`,
  `LoggingBehavior`) cuando el número de casos de uso crezca.
* **Eventos de dominio + patrón Outbox** para integrarse con mensajería
  (RabbitMQ / Azure Service Bus) sin acoplamiento.
* **Autenticación JWT** + autorización por políticas en `Infrastructure`.
* **Cache distribuida** (Redis) para `/standings`, que es read-heavy.
* **Rate limiting** y CORS restringido a dominios específicos en prod.
* **OpenTelemetry** (trazas + métricas) y Serilog con sinks.
* **Health checks** (`/health/live`, `/health/ready`) para Kubernetes.
* **Tests de integración con Testcontainers** (SQL Server real efímero).
* **Tests funcionales con `WebApplicationFactory`** sobre la pipeline completa.

### Frontend

* **Cobertura de tests** con Vitest sobre componentes y servicios.
* **Refresco optimista** (actualizar el listado antes de la respuesta del
  servidor) para CRUDs.
* **Tema oscuro** y persistencia en `localStorage`.
* **i18n** con `@angular/localize` (preparado: textos visibles, no
  identificadores).
* **Estado global con NgRx Signals** si crecen las features.
* **PWA** (offline support y mejor instalación en mobile).
* **E2E con Playwright** sobre los flujos críticos.

### Infraestructura

* **Docker Compose** con API + SQL Server + Nginx + frontend buildado.
* **Pipeline CI/CD** (GitHub Actions) con `build → test → coverage → deploy`.
* **Migraciones EF Core** si en el futuro el equipo prefiere code-first
  (cambiando la fuente de verdad del esquema).
* **Secret management** (Azure Key Vault / AWS Secrets Manager) para connection
  strings y claves.

---

## Licencia y autoría

Prueba técnica desarrollada por **Juan Velasquez**
Uso interno.
