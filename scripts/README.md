# scripts/

Catálogo de scripts automatizables del proyecto.

## Estructura

```
scripts/
├── db/         # Scripts de base de datos (migraciones, seed, backup, restore)
├── utils/      # Utilitarios (activar entorno, instalar SDKs portables, lint, format)
└── deploy/     # Empaquetado, publicación y despliegue
```

## Convenciones

* Scripts PowerShell con extensión `.ps1`, encabezado de ayuda comment-based (`<# .SYNOPSIS ... #>`).
* Scripts Bash con extensión `.sh`, shebang `#!/usr/bin/env bash` y `set -euo pipefail`.
* Todos los scripts deben funcionar **desde la raíz del repositorio**.
* Ningún script debe modificar variables de entorno permanentes ni instalar software fuera de `herramientas/`.

## Scripts incluidos

### utils/

| Script                  | Descripción                                                   |
| ----------------------- | ------------------------------------------------------------- |
| `activate-env.ps1`      | Activa Node y .NET portables en el `PATH` de la sesión actual |
| `setup-tools.ps1`       | (Re)descarga e instala las herramientas portables             |

### db/

| Script                  | Descripción                                                   |
| ----------------------- | ------------------------------------------------------------- |
| *(pendiente)*           | `create-database.ps1`, `apply-migrations.ps1`, `seed.ps1`     |

### deploy/

| Script                  | Descripción                                                   |
| ----------------------- | ------------------------------------------------------------- |
| *(pendiente)*           | `publish-api.ps1`, `build-frontend.ps1`                       |

Las carpetas `db/` y `deploy/` se irán poblando a medida que avance el proyecto.
