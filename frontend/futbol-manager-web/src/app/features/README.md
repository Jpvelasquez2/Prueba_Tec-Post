# features/

Cada feature es un **módulo autocontenido** que representa un dominio funcional de la SPA.

## Convención por feature

```
features/<nombre>/
├── <nombre>.routes.ts         # Rutas internas + loadChildren
├── pages/                     # Smart components (asociados a rutas)
├── components/                # Dumb components privados del feature
├── data-access/               # HTTP services, signal stores, mappers
├── models/                    # Modelos de dominio y DTOs del feature
└── guards/                    # CanActivate / CanDeactivate específicos
```

## Features previstas

| Feature       | Descripción                                                        |
| ------------- | ------------------------------------------------------------------ |
| `equipos/`    | CRUD de equipos y plantilla                                        |
| `jugadores/`  | CRUD de jugadores y asignación a equipos                           |
| `partidos/`   | Calendario, programación y registro de resultados                  |
| `torneos/`    | Creación de torneos y tablas de posiciones                         |
| `dashboard/`  | Vista resumen (próximos partidos, estadísticas)                    |

## Reglas

* Una feature **NUNCA** importa código de otra feature directamente.
* Comunicación entre features vía router (parámetros, query params) o servicio en `core/`.
* Carga **lazy** desde `app.routes.ts` con `loadChildren`.
