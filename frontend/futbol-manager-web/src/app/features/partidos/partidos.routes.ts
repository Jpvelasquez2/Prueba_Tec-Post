import { Routes } from '@angular/router';

/**
 * Rutas internas del feature Partidos.
 *
 * - `''`           → listado con filtros
 * - `'new'`        → formulario de programación
 * - `':id/edit'`   → formulario de edición
 * - `':id/result'` → registrar resultado
 */
export const PARTIDOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/partidos-list/partidos-list.component').then(m => m.PartidosListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./pages/partido-form/partido-form.component').then(m => m.PartidoFormComponent),
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./pages/partido-form/partido-form.component').then(m => m.PartidoFormComponent),
  },
  {
    path: ':id/result',
    loadComponent: () =>
      import('./pages/partido-result/partido-result.component').then(m => m.PartidoResultComponent),
  },
];
