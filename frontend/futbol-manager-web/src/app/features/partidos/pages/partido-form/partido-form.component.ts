import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '../../../../core/ui/toast/toast.service';
import { FormErrorComponent } from '../../../../shared/ui/form-error.component';
import { EquiposService } from '../../../equipos/data-access/equipos.service';
import { Equipo } from '../../../equipos/models/equipo.model';
import { PartidosService } from '../../data-access/partidos.service';
import { EstadoPartido, ETIQUETAS_ESTADO } from '../../models/partido.model';

/**
 * Formulario reactivo para programar o editar un partido.
 *
 * Reglas reflejadas en validaciones:
 *  - Fecha, equipo local, equipo visitante y estado son obligatorios.
 *  - Local y visitante NO pueden ser el mismo equipo (validador cruzado).
 *
 * El backend valida lo mismo y, además, rechaza editar un partido cancelado.
 */
@Component({
  selector: 'app-partido-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, FormErrorComponent],
  template: `
    <section class="max-w-2xl mx-auto space-y-4">
      <header>
        <a routerLink="/partidos" class="text-sm text-slate-500 hover:text-slate-700">← Volver</a>
        <h1 class="mt-2 text-2xl font-bold text-slate-900">{{ titulo() }}</h1>
      </header>

      <form [formGroup]="form" (ngSubmit)="guardar()"
            class="bg-white border border-slate-200 rounded-xl shadow-sm p-6 space-y-5">

        <div>
          <label for="fecha" class="block text-sm font-medium text-slate-700 mb-1">
            Fecha y hora <span class="text-rose-500">*</span>
          </label>
          <input id="fecha" type="datetime-local" formControlName="fecha"
                 [class]="inputClass('fecha')" />
          <app-form-error [control]="form.controls.fecha" />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label for="local" class="block text-sm font-medium text-slate-700 mb-1">
              Equipo local <span class="text-rose-500">*</span>
            </label>
            <select id="local" formControlName="localTeamId" [class]="inputClass('localTeamId')">
              <option [ngValue]="null">Selecciona…</option>
              @for (e of equipos(); track e.id) {
                <option [ngValue]="e.id">{{ e.nombre }}</option>
              }
            </select>
            <app-form-error [control]="form.controls.localTeamId" />
          </div>
          <div>
            <label for="visitante" class="block text-sm font-medium text-slate-700 mb-1">
              Equipo visitante <span class="text-rose-500">*</span>
            </label>
            <select id="visitante" formControlName="visitanteTeamId" [class]="inputClass('visitanteTeamId')">
              <option [ngValue]="null">Selecciona…</option>
              @for (e of equipos(); track e.id) {
                <option [ngValue]="e.id">{{ e.nombre }}</option>
              }
            </select>
            <app-form-error [control]="form.controls.visitanteTeamId" />
          </div>
        </div>

        <!-- Error a nivel de form: equipos iguales. -->
        @if (form.errors?.['mismoEquipo'] && form.touched) {
          <p class="text-sm text-rose-600 -mt-2">Un equipo no puede jugar contra sí mismo.</p>
        }

        <div>
          <label for="estado" class="block text-sm font-medium text-slate-700 mb-1">
            Estado <span class="text-rose-500">*</span>
          </label>
          <select id="estado" formControlName="estado" [class]="inputClass('estado')">
            @for (op of opcionesEstado; track op.value) {
              <option [ngValue]="op.value">{{ op.label }}</option>
            }
          </select>
        </div>

        <div class="flex flex-col-reverse sm:flex-row gap-2 sm:justify-end pt-2 border-t border-slate-100">
          <a routerLink="/partidos"
             class="px-4 py-2 rounded-lg border border-slate-300 text-slate-700 text-sm font-medium hover:bg-slate-50 text-center">
            Cancelar
          </a>
          <button type="submit" [disabled]="guardando()"
                  class="px-4 py-2 rounded-lg bg-emerald-600 text-white text-sm font-medium shadow-sm hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 disabled:opacity-50">
            {{ guardando() ? 'Guardando…' : 'Guardar' }}
          </button>
        </div>
      </form>
    </section>
  `,
})
export class PartidoFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly partidosService = inject(PartidosService);
  private readonly equiposService = inject(EquiposService);
  private readonly toastService = inject(ToastService);

  /** Id del partido en modo edición. */
  private readonly id = signal<number | null>(null);

  /** Bloquea el botón submit mientras hay request en vuelo. */
  readonly guardando = signal(false);

  /** Equipos para alimentar los selects. */
  readonly equipos = signal<Equipo[]>([]);

  /** Título dinámico según el modo. */
  readonly titulo = computed(() => this.id() === null ? 'Programar partido' : 'Editar partido');

  /** Opciones del select de estado. */
  readonly opcionesEstado = [
    { value: EstadoPartido.Programado, label: ETIQUETAS_ESTADO[EstadoPartido.Programado] },
    { value: EstadoPartido.Jugado,     label: ETIQUETAS_ESTADO[EstadoPartido.Jugado] },
    { value: EstadoPartido.Suspendido, label: ETIQUETAS_ESTADO[EstadoPartido.Suspendido] },
    { value: EstadoPartido.Cancelado,  label: ETIQUETAS_ESTADO[EstadoPartido.Cancelado] },
  ];

  /**
   * Form group. Usa `nonNullable` para que los valores nunca sean undefined,
   * salvo los selects de equipo (que inician en null hasta que el usuario elige).
   */
  readonly form = this.fb.group({
    fecha:           this.fb.nonNullable.control('', [Validators.required]),
    localTeamId:     this.fb.control<number | null>(null, [Validators.required]),
    visitanteTeamId: this.fb.control<number | null>(null, [Validators.required]),
    estado:          this.fb.nonNullable.control<EstadoPartido>(EstadoPartido.Programado, [Validators.required]),
  }, { validators: [equiposDistintosValidator] });

  ngOnInit(): void {
    this.cargarEquipos();

    // Si la ruta trae `:id`, cargamos el partido para precargar el form.
    const param = this.route.snapshot.paramMap.get('id');
    if (!param) return;

    const id = Number(param);
    this.id.set(id);

    this.partidosService.obtenerPorId(id).subscribe(p => {
      this.form.patchValue({
        // Convertir ISO (con zona) al formato `datetime-local` (sin zona).
        fecha: p.fecha.substring(0, 16),
        localTeamId: p.localTeamId,
        visitanteTeamId: p.visitanteTeamId,
        estado: p.estado,
      });
    });
  }

  /** Carga el catálogo de equipos para los selects. */
  private cargarEquipos(): void {
    this.equiposService.listar().subscribe(lista => this.equipos.set(lista));
  }

  /** Handler de submit: valida, dispara la mutación y navega a la lista. */
  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastService.warning('Revisa los campos marcados.');
      return;
    }

    this.guardando.set(true);
    const v = this.form.getRawValue();
    const payload = {
      fecha: new Date(v.fecha).toISOString(),
      localTeamId: v.localTeamId!,
      visitanteTeamId: v.visitanteTeamId!,
      estado: v.estado,
    };

    const idActual = this.id();
    const req = idActual === null
      ? this.partidosService.crear(payload)
      : this.partidosService.actualizar(idActual, payload);

    req.subscribe({
      next: () => {
        this.toastService.success(idActual === null ? 'Partido programado.' : 'Partido actualizado.');
        this.router.navigate(['/partidos']);
      },
      error: () => this.guardando.set(false),
      complete: () => this.guardando.set(false),
    });
  }

  /** Clases del input según estado de validación del control. */
  inputClass(field: 'fecha' | 'localTeamId' | 'visitanteTeamId' | 'estado'): string {
    const c = this.form.controls[field];
    const base = 'block w-full rounded-lg border px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2';
    return c.touched && c.invalid
      ? `${base} border-rose-300 focus:ring-rose-400`
      : `${base} border-slate-300 focus:ring-emerald-500 focus:border-emerald-500`;
  }
}

/**
 * Validador cruzado: el equipo local y el visitante deben ser distintos.
 * Si alguno es null, se ignora (lo cubren los `Validators.required` por campo).
 */
const equiposDistintosValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const local = control.get('localTeamId')?.value;
  const visitante = control.get('visitanteTeamId')?.value;
  if (local && visitante && local === visitante) {
    return { mismoEquipo: true };
  }
  return null;
};
