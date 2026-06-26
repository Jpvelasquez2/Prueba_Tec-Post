import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { EmptyStateComponent } from '../../../../shared/ui/empty-state.component';
import { PosicionesService } from '../../data-access/posiciones.service';
import { Posicion } from '../../models/posicion.model';

/**
 * Página de la tabla de posiciones.
 *
 * Muestra las columnas requeridas: Posición, Equipo, PJ, PG, PE, PP, GF, GC,
 * DG y PTS. Incluye un filtro por nombre de equipo (búsqueda incremental)
 * que se aplica en memoria.
 *
 * El backend ya entrega la tabla ordenada por PTS desc, DG desc, GF desc,
 * Nombre asc — el componente no reordena.
 */
@Component({
  selector: 'app-posiciones-page',
  standalone: true,
  imports: [ReactiveFormsModule, EmptyStateComponent],
  template: `
    <section class="space-y-4">
      <header class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-slate-900">Tabla de posiciones</h1>
          <p class="text-sm text-slate-600">
            Ordenado por puntos, diferencia de goles y goles a favor.
          </p>
        </div>
        <div class="w-full sm:w-72">
          <label class="block text-xs font-medium text-slate-600 mb-1">Filtrar por equipo</label>
          <input type="text" [formControl]="busqueda"
                 placeholder="Buscar nombre de equipo…"
                 class="block w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500" />
        </div>
      </header>

      <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-slate-200 text-sm">
            <thead class="bg-slate-50 text-slate-600 uppercase text-xs tracking-wider">
              <tr>
                <th class="px-3 py-3 text-center w-10">#</th>
                <th class="px-3 py-3 text-left">Equipo</th>
                <th class="px-3 py-3 text-center" title="Partidos Jugados">PJ</th>
                <th class="px-3 py-3 text-center" title="Partidos Ganados">PG</th>
                <th class="px-3 py-3 text-center" title="Partidos Empatados">PE</th>
                <th class="px-3 py-3 text-center" title="Partidos Perdidos">PP</th>
                <th class="px-3 py-3 text-center" title="Goles a Favor">GF</th>
                <th class="px-3 py-3 text-center" title="Goles en Contra">GC</th>
                <th class="px-3 py-3 text-center" title="Diferencia de Goles">DG</th>
                <th class="px-3 py-3 text-center font-bold text-emerald-700" title="Puntos">PTS</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              @for (fila of filtradas(); track fila.equipoId) {
                <tr class="hover:bg-slate-50">
                  <td class="px-3 py-3 text-center font-semibold text-slate-500">{{ fila.posicion }}</td>
                  <td class="px-3 py-3">
                    <div class="flex items-center gap-3">
                      @if (fila.escudoUrl) {
                        <img [src]="fila.escudoUrl" [alt]="fila.equipo"
                             class="w-7 h-7 rounded-full object-cover border border-slate-200" />
                      } @else {
                        <div class="w-7 h-7 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 text-xs">
                          {{ iniciales(fila.equipo) }}
                        </div>
                      }
                      <div>
                        <p class="font-medium text-slate-900">{{ fila.equipo }}</p>
                        @if (fila.ciudad) {
                          <p class="text-xs text-slate-500">{{ fila.ciudad }}</p>
                        }
                      </div>
                    </div>
                  </td>
                  <td class="px-3 py-3 text-center">{{ fila.pj }}</td>
                  <td class="px-3 py-3 text-center text-emerald-700 font-medium">{{ fila.pg }}</td>
                  <td class="px-3 py-3 text-center text-slate-600">{{ fila.pe }}</td>
                  <td class="px-3 py-3 text-center text-rose-600">{{ fila.pp }}</td>
                  <td class="px-3 py-3 text-center">{{ fila.gf }}</td>
                  <td class="px-3 py-3 text-center">{{ fila.gc }}</td>
                  <td class="px-3 py-3 text-center"
                      [class]="fila.dg > 0 ? 'text-emerald-700' : fila.dg < 0 ? 'text-rose-600' : 'text-slate-500'">
                    {{ fila.dg > 0 ? '+' + fila.dg : fila.dg }}
                  </td>
                  <td class="px-3 py-3 text-center font-bold text-slate-900">{{ fila.pts }}</td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="10">
                    <app-empty-state
                      icon="🏆"
                      title="No hay datos para mostrar"
                      [message]="textoVacio()" />
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
export class PosicionesPageComponent implements OnInit {
  private readonly posicionesService = inject(PosicionesService);

  /** Tabla completa devuelta por la API. */
  readonly tabla = signal<Posicion[]>([]);

  /** Control reactivo del input de búsqueda. */
  readonly busqueda = new FormControl<string>('', { nonNullable: true });

  /** Convertimos el valor del filtro a signal para componer con `computed`. */
  private readonly busquedaSignal = toSignal(this.busqueda.valueChanges, { initialValue: '' });

  /** Filas filtradas por nombre (case-insensitive). */
  readonly filtradas = computed<Posicion[]>(() => {
    const q = this.busquedaSignal().trim().toLowerCase();
    const datos = this.tabla();
    if (!q) return datos;
    return datos.filter(fila => fila.equipo.toLowerCase().includes(q));
  });

  /** Mensaje del estado vacío. Cambia según haya o no filtro activo. */
  readonly textoVacio = computed(() =>
    this.busquedaSignal().trim()
      ? 'Ningún equipo coincide con la búsqueda.'
      : 'Aún no se han registrado equipos o partidos jugados.');

  ngOnInit(): void {
    this.posicionesService.obtener().subscribe(filas => this.tabla.set(filas));
  }

  /** Iniciales del nombre como fallback del escudo. */
  iniciales(nombre: string): string {
    return nombre.split(/\s+/).filter(Boolean).slice(0, 2).map(w => w[0]).join('').toUpperCase();
  }
}
