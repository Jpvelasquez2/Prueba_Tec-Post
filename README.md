# Prueba_Tec-Post — Gestión de Equipos y Partidos de Fútbol

Solución full-stack que sirve como base para la prueba técnica.

| Capa     | Tecnología                                           |
| -------- | ---------------------------------------------------- |
| Backend  | ASP.NET Core 8 LTS · EF Core · SQL Server · MediatR  |
| Frontend | Angular 21 LTS · TypeScript · Tailwind CSS · RxJS    |
| BD       | SQL Server                                           |

## Estructura

```
Prueba_Tec-Post/
├── ARQUITECTURA.md         # Diseño arquitectónico completo
├── backend/                # Solución .NET (Clean Architecture)
├── frontend/               # Workspace Angular (Feature-Based)
├── herramientas/           # SDKs portables (Node.js + .NET)
└── scripts/                # Scripts de db, utilitarios y despliegue
```

## Primeros pasos

1. **Activar las herramientas portables** (no necesitas instalar nada globalmente):

   ```powershell
   .\scripts\utils\activate-env.ps1
   ```

   Esto agrega `herramientas\dotnet-sdk` y `herramientas\nodejs` al `PATH` de la sesión actual.

2. **Verificar**:

   ```powershell
   dotnet --version
   node --version
   ```

3. **Backend**:

   ```powershell
   cd backend
   dotnet restore
   dotnet build
   ```

4. **Frontend**:

   ```powershell
   cd frontend\futbol-manager-web
   npm install
   npm start
   ```

## Documentación

* [ARQUITECTURA.md](ARQUITECTURA.md) — diseño completo (capas, flujo, convenciones).
* `backend/README.md` — descripción de la solución .NET y cada proyecto.
* `frontend/futbol-manager-web/README.md` — descripción del SPA.
* `herramientas/README.md` — cómo usar los SDKs portables.
* `scripts/README.md` — catálogo de scripts.

> **Estado actual:** solo estructura base, sin funcionalidades implementadas.
