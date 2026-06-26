# herramientas/

SDKs portables incluidos con el repositorio para evitar instalaciones globales.

## Contenido

| Carpeta       | Descripción                                | Versión        |
| ------------- | ------------------------------------------ | -------------- |
| `nodejs/`     | Node.js LTS (incluye `npm` y `npx`)        | 24.18.0 LTS    |
| `dotnet-sdk/` | .NET SDK LTS (incluye `dotnet` y runtime)  | 8.x LTS        |

## Activación rápida

Desde la raíz del repositorio, en PowerShell:

```powershell
.\scripts\utils\activate-env.ps1
```

Eso agrega ambas carpetas al `PATH` **solo en la sesión actual** (no modifica nada del sistema).

Para verificar:

```powershell
dotnet --version
node --version
npm --version
```

## ¿Por qué portable?

* No requiere derechos de administrador.
* Garantiza versión exacta para todos los desarrolladores.
* La solución se puede mover entre máquinas sin reinstalar nada.
* Evita conflictos con otras versiones ya instaladas.

## Actualización

Para regenerar las herramientas, ejecutar:

```powershell
.\scripts\utils\setup-tools.ps1
```

Ese script descarga nuevamente Node LTS y .NET SDK LTS en sus carpetas correspondientes.
