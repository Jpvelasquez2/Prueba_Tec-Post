import { Component, computed, input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

/**
 * Componente que muestra mensajes de validación para un control de
 * Reactive Forms. Renderiza únicamente cuando el control está `invalid` y
 * ha sido `touched` (evita gritarle al usuario antes de que escriba).
 *
 * Soporta los errores nativos más comunes; para reglas custom puedes
 * proporcionar `customMessages`.
 *
 * @example
 * ```html
 * <input [formControl]="form.controls.nombre" />
 * <app-form-error [control]="form.controls.nombre" />
 * ```
 */
@Component({
  selector: 'app-form-error',
  standalone: true,
  template: `
    @if (mensaje(); as msg) {
      <p class="mt-1 text-xs text-rose-600" role="alert">{{ msg }}</p>
    }
  `,
})
export class FormErrorComponent {
  /** Control reactivo a observar. */
  readonly control = input.required<AbstractControl | null>();

  /** Mapa opcional para sobreescribir mensajes por código de error. */
  readonly customMessages = input<Record<string, string>>({});

  /**
   * Texto del error a mostrar; `null` si no procede.
   * Solo se calcula cuando el control fue tocado para evitar parpadeo.
   */
  readonly mensaje = computed<string | null>(() => {
    const c = this.control();
    if (!c || !c.touched || !c.errors) return null;

    const errores = c.errors;
    const custom = this.customMessages();

    // Buscar primero en los mensajes custom.
    for (const clave of Object.keys(errores)) {
      if (custom[clave]) return custom[clave];
    }

    // Mensajes por defecto.
    if (errores['required'])  return 'Este campo es obligatorio.';
    if (errores['minlength']) return `Mínimo ${errores['minlength'].requiredLength} caracteres.`;
    if (errores['maxlength']) return `Máximo ${errores['maxlength'].requiredLength} caracteres.`;
    if (errores['min'])       return `Valor mínimo: ${errores['min'].min}.`;
    if (errores['max'])       return `Valor máximo: ${errores['max'].max}.`;
    if (errores['pattern'])   return 'Formato inválido.';
    if (errores['email'])     return 'Correo no válido.';

    return 'Valor inválido.';
  });
}
