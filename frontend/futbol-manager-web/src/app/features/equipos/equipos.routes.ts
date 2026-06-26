import { Routes } from '@angular/router';

/**
 * Rutas internas del feature Equipos.
 *
 * - `''`           → listado
 * - `'new'`        → formulario en modo creación
 * - `':id/edit'`   → formulario en modo edición
 */
export const EQUIPOS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/equipos-list/equipos-list.component').then(m => m.EquiposListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./pages/equipo-form/equipo-form.component').then(m => m.EquipoFormComponent),
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./pages/equipo-form/equipo-form.component').then(m => m.EquipoFormComponent),
  },
];
