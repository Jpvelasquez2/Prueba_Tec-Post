import { Component, computed, inject } from '@angular/core';
import { ConfirmService } from './confirm.service';

/**
 * Modal de confirmación reutilizable.
 *
 * Reacciona al `signal` `active` de {@link ConfirmService}: cuando hay una
 * solicitud, renderiza un backdrop + tarjeta centrada con los botones
 * Cancelar / Confirmar. El estilo del botón Confirmar cambia a rojo si la
 * operación es marcada como `destructive` (borrados).
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  template: `
    @if (current(); as c) {
      <div class="fixed inset-0 z-40 flex items-center justify-center p-4 bg-slate-900/60"
           role="dialog" aria-modal="true" (click)="onBackdropClick($event)">
        <div class="w-full max-w-md bg-white rounded-xl shadow-2xl overflow-hidden animate-confirm-in"
             (click)="$event.stopPropagation()">
          <div class="p-6">
            <h2 class="text-lg font-semibold text-slate-900">{{ c.title }}</h2>
            <p class="mt-2 text-sm text-slate-600 whitespace-pre-line">{{ c.message }}</p>
          </div>
          <div class="px-6 py-4 bg-slate-50 flex justify-end gap-2 border-t border-slate-200">
            <button type="button"
                    class="px-4 py-2 text-sm font-medium rounded-lg border border-slate-300 text-slate-700 hover:bg-white"
                    (click)="cancel()">
              {{ c.cancelText ?? 'Cancelar' }}
            </button>
            <button type="button"
                    class="px-4 py-2 text-sm font-medium rounded-lg text-white"
                    [class]="confirmButtonClass()"
                    (click)="confirm()">
              {{ c.confirmText ?? 'Confirmar' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes confirm-in { from { opacity: 0; transform: scale(0.96); } to { opacity: 1; transform: scale(1); } }
    .animate-confirm-in { animation: confirm-in 0.15s ease-out; }
  `],
})
export class ConfirmDialogComponent {
  private readonly confirmService = inject(ConfirmService);

  /** Solicitud de confirmación activa (o `null`). */
  readonly current = this.confirmService.active;

  /** Clases del botón principal según sea destructivo o no. */
  readonly confirmButtonClass = computed(() =>
    this.current()?.destructive
      ? 'bg-rose-600 hover:bg-rose-700 focus:ring-2 focus:ring-rose-500'
      : 'bg-emerald-600 hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500');

  /** Cierra el diálogo confirmando. */
  confirm(): void { this.confirmService.confirm(); }

  /** Cierra el diálogo cancelando. */
  cancel(): void { this.confirmService.cancel(); }

  /**
   * Cancela cuando el usuario hace click fuera de la tarjeta (en el backdrop).
   * El stopPropagation del card evita que clicks dentro lo disparen.
   */
  onBackdropClick(_event: MouseEvent): void { this.cancel(); }
}
