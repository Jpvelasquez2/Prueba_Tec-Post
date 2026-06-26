import { Routes } from '@angular/router';

/**
 * Rutas internas del feature Posiciones. Una sola página.
 */
export const POSICIONES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/posiciones-page/posiciones-page.component').then(m => m.PosicionesPageComponent),
  },
];
