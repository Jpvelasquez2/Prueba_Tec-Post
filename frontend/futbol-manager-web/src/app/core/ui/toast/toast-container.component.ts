import { Component, computed, inject } from '@angular/core';
import { ToastService, ToastType } from './toast.service';

/**
 * Contenedor visual de notificaciones tipo toast.
 *
 * Lee el `signal` de {@link ToastService} y renderiza cada notificación como
 * una tarjeta apilada en la esquina inferior derecha. Se monta una sola vez
 * desde el layout principal.
 */
@Component({
  selector: 'app-toast-container',
  standalone: true,
  template: `
    <div class="fixed bottom-4 right-4 z-50 flex flex-col gap-2 w-80 max-w-[calc(100vw-2rem)]"
         aria-live="polite" aria-atomic="true">
      @for (t of toasts(); track t.id) {
        <div role="alert"
             class="flex items-start gap-3 rounded-lg shadow-lg p-4 text-sm border animate-toast-in"
             [class]="styles()[t.type]">
          <span class="text-lg leading-none">{{ icon(t.type) }}</span>
          <p class="flex-1 break-words">{{ t.message }}</p>
          <button type="button"
                  class="text-current opacity-60 hover:opacity-100"
                  (click)="dismiss(t.id)"
                  aria-label="Cerrar notificación">✕</button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes toast-in { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
    .animate-toast-in { animation: toast-in 0.18s ease-out; }
  `],
})
export class ToastContainerComponent {
  private readonly toastService = inject(ToastService);

  /** Lista actual de toasts (vista de solo lectura). */
  readonly toasts = this.toastService.toasts;

  /** Clases de Tailwind por tipo, calculadas una sola vez. */
  readonly styles = computed<Record<ToastType, string>>(() => ({
    success: 'bg-emerald-50 border-emerald-200 text-emerald-900',
    error:   'bg-rose-50 border-rose-200 text-rose-900',
    info:    'bg-sky-50 border-sky-200 text-sky-900',
    warning: 'bg-amber-50 border-amber-200 text-amber-900',
  }));

  /**
   * Devuelve un icono mono-carácter para un tipo dado.
   * Mantenemos texto plano para evitar dependencias en una librería de iconos.
   */
  icon(type: ToastType): string {
    switch (type) {
      case 'success': return '✓';
      case 'error':   return '✕';
      case 'warning': return '!';
      case 'info':    return 'i';
    }
  }

  /** Cierra un toast manualmente. */
  dismiss(id: number): void {
    this.toastService.dismiss(id);
  }
}
