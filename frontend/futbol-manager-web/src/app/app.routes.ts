import { Routes } from '@angular/router';

/**
 * Tabla de rutas raíz. Cada feature se carga lazy desde su `*.routes.ts`.
 *
 * Por defecto se redirige a `/posiciones`, que es la pantalla más informativa
 * para un usuario que entra a la app sin contexto previo.
 */
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'posiciones' },

  {
    path: 'equipos',
    loadChildren: () => import('./features/equipos/equipos.routes').then(m => m.EQUIPOS_ROUTES),
  },
  {
    path: 'partidos',
    loadChildren: () => import('./features/partidos/partidos.routes').then(m => m.PARTIDOS_ROUTES),
  },
  {
    path: 'posiciones',
    loadChildren: () => import('./features/posiciones/posiciones.routes').then(m => m.POSICIONES_ROUTES),
  },

  // Fallback: cualquier ruta desconocida vuelve al inicio.
  { path: '**', redirectTo: 'posiciones' },
];
