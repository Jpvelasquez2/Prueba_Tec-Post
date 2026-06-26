import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '../../../../core/ui/toast/toast.service';
import { FormErrorComponent } from '../../../../shared/ui/form-error.component';
import { EquiposService } from '../../data-access/equipos.service';

/**
 * Formulario reactivo para crear o editar un equipo.
 *
 * El modo se infiere del `:id` en la URL:
 *  - Sin `:id` → modo creación (`POST /api/teams`).
 *  - Con `:id` → modo edición (carga equipo y `PUT /api/teams/{id}`).
 *
 * Las validaciones están alineadas con las del backend:
 *  - `nombre`: requerido, 1-100 caracteres.
 *  - `ciudad`: opcional, hasta 100 caracteres.
 *  - `escudoUrl`: opcional, URL `http(s)://`, hasta 500 caracteres.
 */
@Component({
  selector: 'app-equipo-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FormErrorComponent],
  template: `
    <section class="max-w-2xl mx-auto space-y-4">
      <header>
        <a routerLink="/equipos" class="text-sm text-slate-500 hover:text-slate-700">← Volver</a>
        <h1 class="mt-2 text-2xl font-bold text-slate-900">{{ titulo() }}</h1>
      </header>

      <form [formGroup]="form" (ngSubmit)="guardar()"
            class="bg-white border border-slate-200 rounded-xl shadow-sm p-6 space-y-5">

        <div>
          <label for="nombre" class="block text-sm font-medium text-slate-700 mb-1">
            Nombre <span class="text-rose-500">*</span>
          </label>
          <input id="nombre" type="text" formControlName="nombre"
                 placeholder="p. ej. Atlético Nacional"
                 [class]="inputClass('nombre')" />
          <app-form-error [control]="form.controls.nombre" />
        </div>

        <div>
          <label for="ciudad" class="block text-sm font-medium text-slate-700 mb-1">Ciudad</label>
          <input id="ciudad" type="text" formControlName="ciudad"
                 placeholder="Opcional" [class]="inputClass('ciudad')" />
          <app-form-error [control]="form.controls.ciudad" />
        </div>

        <div>
          <label for="escudoUrl" class="block text-sm font-medium text-slate-700 mb-1">URL del escudo</label>
          <input id="escudoUrl" type="url" formControlName="escudoUrl"
                 placeholder="https://..." [class]="inputClass('escudoUrl')" />
          <app-form-error [control]="form.controls.escudoUrl"
                          [customMessages]="{ pattern: 'Debe iniciar con http:// o https://' }" />
        </div>

        <div class="flex flex-col-reverse sm:flex-row gap-2 sm:justify-end pt-2 border-t border-slate-100">
          <a routerLink="/equipos"
             class="px-4 py-2 rounded-lg border border-slate-300 text-slate-700 text-sm font-medium hover:bg-slate-50 text-center">
            Cancelar
          </a>
          <button type="submit"
                  [disabled]="guardando()"
                  class="px-4 py-2 rounded-lg bg-emerald-600 text-white text-sm font-medium shadow-sm hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 disabled:opacity-50">
            {{ guardando() ? 'Guardando…' : 'Guardar' }}
          </button>
        </div>
      </form>
    </section>
  `,
})
export class EquipoFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly equiposService = inject(EquiposService);
  private readonly toastService = inject(ToastService);

  /** Id del equipo en modo edición; `null` si es creación. */
  private readonly id = signal<number | null>(null);

  /** Indicador para deshabilitar el submit mientras hay request en curso. */
  readonly guardando = signal(false);

  /** Título dinámico según modo. */
  readonly titulo = computed(() => this.id() === null ? 'Nuevo equipo' : 'Editar equipo');

  /**
   * Form group con validadores espejados de los DataAnnotations del backend.
   * El patrón de URL acepta nulo o cadena vacía como ausencia de valor.
   */
  readonly form = this.fb.nonNullable.group({
    nombre:    this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    ciudad:    this.fb.control<string | null>(null, [Validators.maxLength(100)]),
    escudoUrl: this.fb.control<string | null>(null, [
      Validators.maxLength(500),
      Validators.pattern(/^https?:\/\/.+/i),
    ]),
  });

  /** Carga el equipo si la ruta trae `:id`. */
  ngOnInit(): void {
    const param = this.route.snapshot.paramMap.get('id');
    if (!param) return;

    const id = Number(param);
    this.id.set(id);

    this.equiposService.obtenerPorId(id).subscribe(equipo => {
      this.form.patchValue({
        nombre: equipo.nombre,
        ciudad: equipo.ciudad,
        escudoUrl: equipo.escudoUrl,
      });
    });
  }

  /** Submit handler: valida, dispara POST/PUT y vuelve a la lista. */
  guardar(): void {
    // Si la forma es inválida marcamos todos los controles para que se muestren
    // los errores visuales (el FormErrorComponent solo dispara con touched).
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastService.warning('Revisa los campos marcados.');
      return;
    }

    this.guardando.set(true);
    const valor = this.form.getRawValue();
    const idActual = this.id();

    const peticion = idActual === null
      ? this.equiposService.crear(valor)
      : this.equiposService.actualizar(idActual, valor);

    peticion.subscribe({
      next: () => {
        this.toastService.success(idActual === null ? 'Equipo creado.' : 'Equipo actualizado.');
        this.router.navigate(['/equipos']);
      },
      error: () => this.guardando.set(false),
      complete: () => this.guardando.set(false),
    });
  }

  /**
   * Devuelve las clases CSS del input para reflejar el estado de validación
   * (borde rojo si el control fue tocado y es inválido).
   */
  inputClass(field: 'nombre' | 'ciudad' | 'escudoUrl'): string {
    const c = this.form.controls[field];
    const base = 'block w-full rounded-lg border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2';
    return c.touched && c.invalid
      ? `${base} border-rose-300 focus:ring-rose-400`
      : `${base} border-slate-300 focus:ring-emerald-500 focus:border-emerald-500`;
  }
}
