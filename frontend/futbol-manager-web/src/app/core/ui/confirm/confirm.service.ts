import { Injectable, signal } from '@angular/core';

/** Configuración de un diálogo de confirmación pendiente. */
export interface ConfirmRequest {
  /** Título corto y descriptivo. */
  readonly title: string;
  /** Mensaje principal mostrado al usuario. */
  readonly message: string;
  /** Texto del botón de confirmación. Por defecto "Confirmar". */
  readonly confirmText?: string;
  /** Texto del botón de cancelación. Por defecto "Cancelar". */
  readonly cancelText?: string;
  /** Si `true`, pinta el botón de confirmación en rojo (operaciones destructivas). */
  readonly destructive?: boolean;
}

/** Estado activo del diálogo. Se vincula a un resolver para devolver el resultado. */
interface ActiveConfirm extends ConfirmRequest {
  readonly resolve: (confirmed: boolean) => void;
}

/**
 * Servicio para solicitar confirmaciones al usuario.
 *
 * El método `ask` devuelve una `Promise<boolean>` que resuelve cuando el
 * usuario hace clic en Confirmar (`true`) o Cancelar/cierra (`false`).
 *
 * Un único `ConfirmDialogComponent` montado en el layout principal observa
 * el `signal` `active` y renderiza el modal cuando hay una solicitud.
 */
@Injectable({ providedIn: 'root' })
export class ConfirmService {
  /** Solicitud activa o `null` si no hay diálogo. */
  private readonly _active = signal<ActiveConfirm | null>(null);

  /** Estado público de solo lectura. */
  readonly active = this._active.asReadonly();

  /**
   * Solicita confirmación al usuario.
   * @returns Promesa que resuelve a `true` si confirma, `false` si cancela.
   */
  ask(request: ConfirmRequest): Promise<boolean> {
    return new Promise<boolean>(resolve => {
      this._active.set({ ...request, resolve });
    });
  }

  /** El diálogo informa que el usuario aceptó. */
  confirm(): void {
    const current = this._active();
    if (!current) return;
    this._active.set(null);
    current.resolve(true);
  }

  /** El diálogo informa que el usuario canceló o cerró. */
  cancel(): void {
    const current = this._active();
    if (!current) return;
    this._active.set(null);
    current.resolve(false);
  }
}
