import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmDialogComponent } from '../ui/confirm/confirm-dialog.component';
import { LoaderBarComponent } from '../ui/loader/loader-bar.component';
import { ToastContainerComponent } from '../ui/toast/toast-container.component';

/**
 * Layout principal de la SPA.
 *
 * Renderiza:
 *  - Header con navegación entre features (Equipos, Partidos, Posiciones).
 *  - Contenedor `<main>` con el `<router-outlet>` para el contenido por ruta.
 *  - Componentes globales: barra de carga, toasts y modal de confirmación.
 *
 * Los componentes globales se montan UNA sola vez aquí; los servicios
 * subyacentes ({@link LoaderService}, {@link ToastService}, {@link ConfirmService})
 * son singletons.
 */
@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LoaderBarComponent, ToastContainerComponent, ConfirmDialogComponent],
  template: `
    <app-loader-bar />

    <header class="bg-white border-b border-slate-200 shadow-sm">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <a routerLink="/" class="flex items-center gap-2 text-emerald-700 font-bold text-lg">
          <span class="text-2xl">⚽</span>
          <span class="hidden sm:inline">FutbolManager</span>
        </a>
        <nav class="flex items-center gap-1 text-sm font-medium">
          <a routerLink="/equipos"
             routerLinkActive="bg-emerald-50 text-emerald-700"
             class="px-3 py-2 rounded-md text-slate-600 hover:bg-slate-100">Equipos</a>
          <a routerLink="/partidos"
             routerLinkActive="bg-emerald-50 text-emerald-700"
             class="px-3 py-2 rounded-md text-slate-600 hover:bg-slate-100">Partidos</a>
          <a routerLink="/posiciones"
             routerLinkActive="bg-emerald-50 text-emerald-700"
             class="px-3 py-2 rounded-md text-slate-600 hover:bg-slate-100">Posiciones</a>
        </nav>
      </div>
    </header>

    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <router-outlet />
    </main>

    <app-toast-container />
    <app-confirm-dialog />
  `,
})
export class MainLayoutComponent {}
