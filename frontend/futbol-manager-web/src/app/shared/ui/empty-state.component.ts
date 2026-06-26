import { Component, input } from '@angular/core';

/**
 * Placeholder visual para mostrar cuando una colección viene vacía.
 *
 * Recibe un título y un mensaje secundario opcionales. Está pensado para usarse
 * dentro de tablas o listas (`<tr><td colspan="…"><app-empty-state …/></td></tr>`).
 */
@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center py-12 text-center text-slate-500">
      <span class="text-4xl mb-3">{{ icon() }}</span>
      <p class="text-base font-medium text-slate-700">{{ title() }}</p>
      @if (message()) {
        <p class="text-sm mt-1">{{ message() }}</p>
      }
    </div>
  `,
})
export class EmptyStateComponent {
  /** Icono mono-carácter (emoji o símbolo Unicode). */
  readonly icon = input<string>('∅');

  /** Título principal del estado vacío. */
  readonly title = input.required<string>();

  /** Mensaje secundario opcional. */
  readonly message = input<string | null>(null);
}
