# shared/

Componentes, pipes, directivas y utilidades **reutilizables** sin dependencias de negocio.

## Contenido

| Subcarpeta     | Responsabilidad                                                      |
| -------------- | -------------------------------------------------------------------- |
| `ui/`          | Componentes presentacionales: `button`, `input`, `modal`, `table`    |
| `pipes/`       | Pipes de formato (fecha, moneda, truncado)                           |
| `directives/`  | Directivas de comportamiento (autofocus, debounce-click, etc.)       |
| `validators/`  | Validators tipados para Reactive Forms                               |
| `utils/`       | Funciones puras de utilidad (mappers, formateadores)                 |

## Reglas

* **Stateless**: nada de servicios con estado ni llamadas HTTP.
* **No depende de features ni de core**.
* Cada componente es `standalone: true`.
