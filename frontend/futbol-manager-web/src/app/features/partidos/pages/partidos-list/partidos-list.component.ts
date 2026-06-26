import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ConfirmService } from '../../../../core/ui/confirm/confirm.service';
import { ToastService } from '../../../../core/ui/toast/toast.service';
import { EmptyStateComponent } from '../../../../shared/ui/empty-state.component';
import { EquiposService } from '../../../equipos/data-access/equipos.service';
import { Equipo } from '../../../equipos/models/equipo.model';
import { PartidosService } from '../../data-access/partidos.service';
import { EstadoPartido, ETIQUETAS_ESTADO, Partido } from '../../models/partido.model';

/**
 * Página de listado de partidos con filtros por equipo, estado y fecha.
 *
 * El filtrado se hace en memoria: la API devuelve la colección completa
 * (es pequeña en este dominio) y un `computed` derivado de los signals
 * de filtros decide qué filas se muestran.
 */
@Component({
  selector: 'app-partidos-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, EmptyStateComponent],
  template: `
    <section class="space-y-4">
      <header class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-slate-900">Partidos</h1>
          <p class="text-sm text-slate-600">Programa, edita y registra resultados.</p>
        </div>
        <a routerLink="/partidos/new"
           class="inline-flex items-center justify-center gap-1.5 px-4 py-2 rounded-lg bg-emerald-600 text-white text-sm font-medium shadow-sm hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500">
          <span>＋</span> Programar partido
        </a>
      </header>

      <!-- Filtros -->
      <form [formGroup]="filtros"
            class="bg-white border border-slate-200 rounded-xl shadow-sm p-4 grid grid-cols-1 sm:grid-cols-4 gap-3">
        <div class="sm:col-span-2">
          <label class="block text-xs font-medium text-slate-600 mb-1">Equipo</label>
          <select formControlName="equipoId" class="block w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
            <option [ngValue]="null">Todos los equipos</option>
            @for (e of equipos(); track e.id) {
              <option [ngValue]="e.id">{{ e.nombre }}</option>
            }
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-slate-600 mb-1">Estado</label>
          <select formControlName="estado" class="block w-full rounded-lg border border-slate-300 px-3 py-2 text-sm">
            <option [ngValue]="null">Todos los estados</option>
            @for (op of opcionesEstado; track op.value) {
              <option [ngValue]="op.value">{{ op.label }}</option>
            }
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-slate-600 mb-1">Fecha</label>
          <input type="date" formControlName="fecha"
                 class="block w-full rounded-lg border border-slate-300 px-3 py-2 text-sm" />
        </div>
        @if (algunFiltroActivo()) {
          <div class="sm:col-span-4 flex justify-end">
            <button type="button" (click)="limpiarFiltros()"
                    class="text-xs text-slate-600 hover:text-slate-900 underline">
              Limpiar filtros
            </button>
          </div>
        }
      </form>

      <!-- Tabla -->
      <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-slate-200 text-sm">
            <thead class="bg-slate-50 text-slate-600 uppercase text-xs tracking-wider">
              <tr>
                <th class="px-4 py-3 text-left">Fecha</th>
                <th class="px-4 py-3 text-left">Local</th>
                <th class="px-4 py-3 text-center">Marcador</th>
                <th class="px-4 py-3 text-left">Visitante</th>
                <th class="px-4 py-3 text-left">Estado</th>
                <th class="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              @for (p of partidosFiltrados(); track p.id) {
                <tr class="hover:bg-slate-50">
                  <td class="px-4 py-3 text-slate-700 whitespace-nowrap">{{ p.fecha | date: 'short' }}</td>
                  <td class="px-4 py-3 font-medium text-slate-900">{{ p.localTeamNombre }}</td>
                  <td class="px-4 py-3 text-center font-mono">
                    @if (p.localScore !== null) {
                      <span class="font-semibold">{{ p.localScore }} - {{ p.visitanteScore }}</span>
                    } @else {
                      <span class="text-slate-400">vs</span>
                    }
                  </td>
                  <td class="px-4 py-3 font-medium text-slate-900">{{ p.visitanteTeamNombre }}</td>
                  <td class="px-4 py-3">
                    <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                          [class]="estadoBadge(p.estado)">
                      {{ etiquetaEstado(p.estado) }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-right space-x-1 whitespace-nowrap">
                    @if (p.estado !== EstadoPartido.Cancelado) {
                      <a [routerLink]="['/partidos', p.id, 'result']"
                         class="inline-flex items-center px-2.5 py-1.5 rounded-md text-xs font-medium bg-emerald-600 text-white hover:bg-emerald-700">
                        Resultado
                      </a>
                    }
                    <a [routerLink]="['/partidos', p.id, 'edit']"
                       class="inline-flex items-center px-2.5 py-1.5 rounded-md text-xs font-medium border border-slate-300 text-slate-700 hover:bg-slate-50">
                      Editar
                    </a>
                    <button type="button"
                            class="inline-flex items-center px-2.5 py-1.5 rounded-md text-xs font-medium border border-rose-300 text-rose-700 hover:bg-rose-50"
                            (click)="eliminar(p)">
                      Eliminar
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="6">
                    <app-empty-state
                      icon="📅"
                      title="No hay partidos para los filtros aplicados"
                      message="Prueba ajustando los filtros o programa un nuevo partido." />
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </section>
  `,
})
export class PartidosListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly partidosService = inject(PartidosService);
  private readonly equiposService = inject(EquiposService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toastService = inject(ToastService);

  /** Expuesto al template para usar el enum en directivas. */
  protected readonly EstadoPartido = EstadoPartido;

  /** Opciones del select de estado, derivadas del enum. */
  protected readonly opcionesEstado = [
    { value: EstadoPartido.Programado, label: ETIQUETAS_ESTADO[EstadoPartido.Programado] },
    { value: EstadoPartido.Jugado,     label: ETIQUETAS_ESTADO[EstadoPartido.Jugado] },
    { value: EstadoPartido.Suspendido, label: ETIQUETAS_ESTADO[EstadoPartido.Suspendido] },
    { value: EstadoPartido.Cancelado,  label: ETIQUETAS_ESTADO[EstadoPartido.Cancelado] },
  ];

  /** Estado interno. */
  readonly partidos = signal<Partido[]>([]);
  readonly equipos = signal<Equipo[]>([]);

  /** Filtros como Reactive Form: cada cambio se proyecta a un signal. */
  readonly filtros = this.fb.nonNullable.group({
    equipoId: this.fb.control<number | null>(null),
    estado:   this.fb.control<EstadoPartido | null>(null),
    fecha:    this.fb.control<string | null>(null),
  });

  /**
   * Convertimos `valueChanges` en signal: cada cambio dispara recálculo
   * de la lista filtrada sin necesidad de subscripciones manuales.
   */
  private readonly filtrosSignal = toSignal(this.filtros.valueChanges, {
    initialValue: this.filtros.getRawValue(),
  });

  /** ¿Hay algún filtro distinto del valor por defecto (null/vacío)? */
  readonly algunFiltroActivo = computed(() => {
    const f = this.filtrosSignal();
    return !!(f.equipoId || f.estado || f.fecha);
  });

  /**
   * Lista filtrada. Aplicamos los filtros en cascada: cada uno reduce la
   * salida del anterior. Comparamos la fecha solo por el día calendario
   * para que el input `date` (sin hora) coincida con cualquier hora del día.
   */
  readonly partidosFiltrados = computed<Partido[]>(() => {
    const lista = this.partidos();
    const f = this.filtrosSignal();
    return lista.filter(p => {
      if (f.equipoId !== null && f.equipoId !== undefined) {
        if (p.localTeamId !== f.equipoId && p.visitanteTeamId !== f.equipoId) return false;
      }
      if (f.estado !== null && f.estado !== undefined) {
        if (p.estado !== f.estado) return false;
      }
      if (f.fecha) {
        const diaPartido = p.fecha.substring(0, 10);  // ISO 'YYYY-MM-DD'
        if (diaPartido !== f.fecha) return false;
      }
      return true;
    });
  });

  ngOnInit(): void {
    this.cargarPartidos();
    this.cargarEquipos();
  }

  /** Lee la colección de partidos desde la API. */
  private cargarPartidos(): void {
    this.partidosService.listar().subscribe(lista => this.partidos.set(lista));
  }

  /** Lee la colección de equipos para alimentar el select del filtro. */
  private cargarEquipos(): void {
    this.equiposService.listar().subscribe(lista => this.equipos.set(lista));
  }

  /** Limpia los tres filtros a sus valores neutros. */
  limpiarFiltros(): void {
    this.filtros.reset({ equipoId: null, estado: null, fecha: null });
  }

  /** Pide confirmación y elimina el partido. */
  async eliminar(partido: Partido): Promise<void> {
    const ok = await this.confirmService.ask({
      title: 'Eliminar partido',
      message: `¿Eliminar el partido ${partido.localTeamNombre} vs ${partido.visitanteTeamNombre}?`,
      confirmText: 'Eliminar',
      destructive: true,
    });
    if (!ok) return;

    this.partidosService.eliminar(partido.id).subscribe(() => {
      this.toastService.success('Partido eliminado.');
      this.cargarPartidos();
    });
  }

  /** Etiqueta legible del estado. */
  etiquetaEstado(estado: EstadoPartido): string { return ETIQUETAS_ESTADO[estado]; }

  /** Clases del badge según estado. */
  estadoBadge(estado: EstadoPartido): string {
    switch (estado) {
      case EstadoPartido.Programado: return 'bg-sky-100 text-sky-800';
      case EstadoPartido.Jugado:     return 'bg-emerald-100 text-emerald-800';
      case EstadoPartido.Suspendido: return 'bg-amber-100 text-amber-800';
      case EstadoPartido.Cancelado:  return 'bg-rose-100 text-rose-800';
    }
  }
}
