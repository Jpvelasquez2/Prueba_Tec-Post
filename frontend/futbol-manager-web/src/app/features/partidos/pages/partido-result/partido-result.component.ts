import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '../../../../core/ui/toast/toast.service';
import { FormErrorComponent } from '../../../../shared/ui/form-error.component';
import { PartidosService } from '../../data-access/partidos.service';
import { Partido } from '../../models/partido.model';

/**
 * Página para registrar el resultado de un partido específico.
 *
 * Es una pantalla minimal: muestra los equipos y un par de inputs numéricos
 * para los goles. Al guardar, el backend transiciona el partido a `Jugado`
 * automáticamente.
 *
 * Reglas de validación espejadas:
 *  - Ambos goles son obligatorios.
 *  - Goles ≥ 0 y ≤ 255 (rango TINYINT).
 */
@Component({
  selector: 'app-partido-result',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, FormErrorComponent],
  template: `
    <section class="max-w-xl mx-auto space-y-4">
      <header>
        <a routerLink="/partidos" class="text-sm text-slate-500 hover:text-slate-700">← Volver</a>
        <h1 class="mt-2 text-2xl font-bold text-slate-900">Registrar resultado</h1>
      </header>

      @if (partido(); as p) {
        <div class="bg-white border border-slate-200 rounded-xl shadow-sm p-6">
          <p class="text-xs uppercase text-slate-500 tracking-wider">{{ p.fecha | date: 'medium' }}</p>

          <form [formGroup]="form" (ngSubmit)="guardar()" class="mt-4 space-y-5">
            <div class="grid grid-cols-3 items-end gap-4">
              <!-- Local -->
              <div class="text-center">
                <p class="text-sm font-semibold text-slate-700 mb-2 truncate">{{ p.localTeamNombre }}</p>
                <input type="number" min="0" max="255" formControlName="localScore"
                       [class]="inputClass('localScore')" />
                <app-form-error [control]="form.controls.localScore" />
              </div>

              <div class="text-3xl font-bold text-slate-400 text-center pb-3">—</div>

              <!-- Visitante -->
              <div class="text-center">
                <p class="text-sm font-semibold text-slate-700 mb-2 truncate">{{ p.visitanteTeamNombre }}</p>
                <input type="number" min="0" max="255" formControlName="visitanteScore"
                       [class]="inputClass('visitanteScore')" />
                <app-form-error [control]="form.controls.visitanteScore" />
              </div>
            </div>

            <div class="flex flex-col-reverse sm:flex-row gap-2 sm:justify-end pt-2 border-t border-slate-100">
              <a routerLink="/partidos"
                 class="px-4 py-2 rounded-lg border border-slate-300 text-slate-700 text-sm font-medium hover:bg-slate-50 text-center">
                Cancelar
              </a>
              <button type="submit" [disabled]="guardando()"
                      class="px-4 py-2 rounded-lg bg-emerald-600 text-white text-sm font-medium shadow-sm hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 disabled:opacity-50">
                {{ guardando() ? 'Guardando…' : 'Guardar resultado' }}
              </button>
            </div>
          </form>
        </div>
      }
    </section>
  `,
})
export class PartidoResultComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly partidosService = inject(PartidosService);
  private readonly toastService = inject(ToastService);

  /** Partido cargado desde la API. `null` mientras carga. */
  readonly partido = signal<Partido | null>(null);

  /** Flag para deshabilitar el submit mientras hay request en vuelo. */
  readonly guardando = signal(false);

  /**
   * Formulario con dos campos numéricos. Iniciamos en `0` para que el input
   * `type=number` tenga un valor inicial razonable y el min/max funcionen.
   */
  readonly form = this.fb.nonNullable.group({
    localScore:     this.fb.nonNullable.control(0, [Validators.required, Validators.min(0), Validators.max(255)]),
    visitanteScore: this.fb.nonNullable.control(0, [Validators.required, Validators.min(0), Validators.max(255)]),
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.partidosService.obtenerPorId(id).subscribe(p => {
      this.partido.set(p);
      // Si el partido ya tenía resultado, lo precargamos.
      if (p.localScore !== null && p.visitanteScore !== null) {
        this.form.patchValue({ localScore: p.localScore, visitanteScore: p.visitanteScore });
      }
    });
  }

  /** Submit handler: POST al endpoint de resultado y navega a la lista. */
  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastService.warning('Goles deben ser ≥ 0 y ≤ 255.');
      return;
    }
    const p = this.partido();
    if (!p) return;

    this.guardando.set(true);
    this.partidosService.registrarResultado(p.id, this.form.getRawValue()).subscribe({
      next: () => {
        this.toastService.success('Resultado registrado. El partido pasó a Jugado.');
        this.router.navigate(['/partidos']);
      },
      error: () => this.guardando.set(false),
      complete: () => this.guardando.set(false),
    });
  }

  /** Clases del input según validación. */
  inputClass(field: 'localScore' | 'visitanteScore'): string {
    const c = this.form.controls[field];
    const base = 'block w-full rounded-lg border px-3 py-3 text-2xl font-bold text-center shadow-sm focus:outline-none focus:ring-2';
    return c.touched && c.invalid
      ? `${base} border-rose-300 focus:ring-rose-400`
      : `${base} border-slate-300 focus:ring-emerald-500 focus:border-emerald-500`;
  }
}
