import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmService } from '../../../../core/ui/confirm/confirm.service';
import { ToastService } from '../../../../core/ui/toast/toast.service';
import { EmptyStateComponent } from '../../../../shared/ui/empty-state.component';
import { EquiposService } from '../../data-access/equipos.service';
import { Equipo } from '../../models/equipo.model';

/**
 * Página de listado de equipos.
 *
 * Muestra una tabla con todos los equipos y acciones por fila (editar /
 * eliminar). Antes de eliminar pide confirmación al usuario vía
 * {@link ConfirmService}.
 */
@Component({
  selector: 'app-equipos-list',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  template: `
    <section class="space-y-4">
      <header class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-slate-900">Equipos</h1>
          <p class="text-sm text-slate-600">Gestiona los equipos participantes.</p>
        </div>
        <a routerLink="/equipos/new"
           class="inline-flex items-center justify-center gap-1.5 px-4 py-2 rounded-lg bg-emerald-600 text-white text-sm font-medium shadow-sm hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500">
          <span>＋</span> Nuevo equipo
        </a>
      </header>

      <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="min-w-full divide-y divide-slate-200 text-sm">
            <thead class="bg-slate-50 text-slate-600 uppercase text-xs tracking-wider">
              <tr>
                <th class="px-4 py-3 text-left w-16">Escudo</th>
                <th class="px-4 py-3 text-left">Nombre</th>
                <th class="px-4 py-3 text-left">Ciudad</th>
                <th class="px-4 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              @for (equipo of equipos(); track equipo.id) {
                <tr class="hover:bg-slate-50">
                  <td class="px-4 py-3">
                    @if (equipo.escudoUrl) {
                      <img [src]="equipo.escudoUrl" [alt]="equipo.nombre"
                           class="w-9 h-9 rounded-full object-cover border border-slate-200" />
                    } @else {
                      <div class="w-9 h-9 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 text-xs">
                        {{ iniciales(equipo.nombre) }}
                      </div>
                    }
                  </td>
                  <td class="px-4 py-3 font-medium text-slate-900">{{ equipo.nombre }}</td>
                  <td class="px-4 py-3 text-slate-600">{{ equipo.ciudad || '—' }}</td>
                  <td class="px-4 py-3 text-right space-x-2">
                    <a [routerLink]="['/equipos', equipo.id, 'edit']"
                       class="inline-flex items-center px-3 py-1.5 rounded-md text-xs font-medium border border-slate-300 text-slate-700 hover:bg-slate-50">
                      Editar
                    </a>
                    <button type="button"
                            class="inline-flex items-center px-3 py-1.5 rounded-md text-xs font-medium border border-rose-300 text-rose-700 hover:bg-rose-50"
                            (click)="eliminar(equipo)">
                      Eliminar
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="4">
                    <app-empty-state
                      icon="🏟️"
                      title="Aún no hay equipos"
                      message="Crea el primero usando el botón “Nuevo equipo”." />
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
export class EquiposListComponent implements OnInit {
  private readonly equiposService = inject(EquiposService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toastService = inject(ToastService);

  /** Lista cargada desde la API. */
  readonly equipos = signal<Equipo[]>([]);

  /** Carga inicial. */
  ngOnInit(): void {
    this.cargar();
  }

  /** Recarga la lista desde la API. */
  private cargar(): void {
    this.equiposService.listar().subscribe({
      next: lista => this.equipos.set(lista),
      // Los errores los maneja el interceptor (toast). Aquí solo dejamos la lista vacía.
    });
  }

  /**
   * Pide confirmación y elimina el equipo. Si la API responde con 409
   * (equipo con partidos asociados), el interceptor mostrará el toast.
   */
  async eliminar(equipo: Equipo): Promise<void> {
    const ok = await this.confirmService.ask({
      title: 'Eliminar equipo',
      message: `¿Confirmas eliminar “${equipo.nombre}”?\nEsta acción no se puede deshacer.`,
      confirmText: 'Eliminar',
      destructive: true,
    });
    if (!ok) return;

    this.equiposService.eliminar(equipo.id).subscribe({
      next: () => {
        this.toastService.success(`Equipo “${equipo.nombre}” eliminado.`);
        this.cargar();
      },
    });
  }

  /** Devuelve las iniciales de un nombre como placeholder de escudo. */
  iniciales(nombre: string): string {
    return nombre
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(w => w[0])
      .join('')
      .toUpperCase();
  }
}
