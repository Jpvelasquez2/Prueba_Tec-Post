import { Injectable, signal } from '@angular/core';

/** Categoría visual de un toast. */
export type ToastType = 'success' | 'error' | 'info' | 'warning';

/** Entrada individual mostrada por el contenedor de toasts. */
export interface Toast {
  /** Identificador único (timestamp + contador). */
  readonly id: number;
  /** Tipo: define color e icono. */
  readonly type: ToastType;
  /** Mensaje visible para el usuario. */
  readonly message: string;
}

/**
 * Servicio centralizado para emitir notificaciones tipo "toast".
 *
 * Mantiene un `signal` con la lista activa de notificaciones; un único
 * `ToastContainerComponent` (montado en el layout principal) las renderiza
 * en una pila fija en la esquina inferior derecha y las descarta tras un
 * intervalo configurable.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  /** Tiempo de vida de un toast por defecto, en ms. */
  private static readonly DEFAULT_DURATION_MS = 4000;

  /** Contador monótono para generar IDs únicos. */
  private nextId = 0;

  /** Stack interno mutable. */
  private readonly _toasts = signal<Toast[]>([]);

  /** Lista pública de solo lectura para el contenedor visual. */
  readonly toasts = this._toasts.asReadonly();

  /**
   * Muestra un toast de éxito.
   * @param message Texto a mostrar.
   * @param durationMs Tiempo opcional en ms hasta auto-cerrarse.
   */
  success(message: string, durationMs = ToastService.DEFAULT_DURATION_MS): void {
    this.push('success', message, durationMs);
  }

  /**
   * Muestra un toast de error.
   * @param message Texto a mostrar (debe ser legible por el usuario final).
   * @param durationMs Tiempo opcional en ms. Por defecto 6000 (errores se leen más despacio).
   */
  error(message: string, durationMs = 6000): void {
    this.push('error', message, durationMs);
  }

  /** Muestra un toast informativo. */
  info(message: string, durationMs = ToastService.DEFAULT_DURATION_MS): void {
    this.push('info', message, durationMs);
  }

  /** Muestra un toast de advertencia. */
  warning(message: string, durationMs = ToastService.DEFAULT_DURATION_MS): void {
    this.push('warning', message, durationMs);
  }

  /** Descarta manualmente un toast por id. */
  dismiss(id: number): void {
    this._toasts.update(list => list.filter(t => t.id !== id));
  }

  /**
   * Encola un toast y programa su auto-cierre.
   * @internal
   */
  private push(type: ToastType, message: string, durationMs: number): void {
    const id = ++this.nextId;
    this._toasts.update(list => [...list, { id, type, message }]);

    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
  }
}
