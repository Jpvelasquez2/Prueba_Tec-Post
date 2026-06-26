import { Component, inject } from '@angular/core';
import { LoaderService } from './loader.service';

/**
 * Barra de progreso indeterminada anclada al borde superior de la pantalla.
 *
 * Aparece mientras haya al menos una operación HTTP en curso (gestionado por
 * {@link LoaderService}). Se monta una sola vez desde el layout principal.
 */
@Component({
  selector: 'app-loader-bar',
  standalone: true,
  template: `
    @if (isLoading()) {
      <div class="fixed top-0 left-0 right-0 h-1 z-50 overflow-hidden bg-emerald-100"
           role="progressbar" aria-label="Cargando">
        <div class="h-full w-1/3 bg-emerald-600 animate-loader-slide"></div>
      </div>
    }
  `,
  styles: [`
    @keyframes loader-slide {
      0%   { transform: translateX(-100%); }
      100% { transform: translateX(400%); }
    }
    .animate-loader-slide { animation: loader-slide 1.1s linear infinite; }
  `],
})
export class LoaderBarComponent {
  private readonly loader = inject(LoaderService);

  /** Indicador reactivo: el componente se renderiza solo si hay carga activa. */
  readonly isLoading = this.loader.isLoading;
}
