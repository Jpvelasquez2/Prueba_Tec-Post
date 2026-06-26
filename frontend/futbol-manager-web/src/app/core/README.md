# core/

Servicios e infraestructura **singleton** de la SPA. Se carga una sola vez al arrancar la app.

## Contenido

| Subcarpeta  | Responsabilidad                                                        |
| ----------- | ---------------------------------------------------------------------- |
| `auth/`     | `AuthService`, `authGuard`, `authInterceptor`, modelos de sesión       |
| `http/`     | `apiBaseService`, interceptores de error y de correlación              |
| `config/`   | Inyección de configuración global (tokens DI, `environment`)           |
| `models/`   | Tipos base: `Result`, `PagedResult`, `ApiError`                        |
| `layout/`   | Layout principal (`MainLayoutComponent`, `Header`, `Sidebar`)          |

## Reglas

* Solo lógica transversal, **nunca código de un dominio de negocio**.
* No depende de `features/` ni de `shared/`.
* Cada servicio es `providedIn: 'root'` (singleton).
