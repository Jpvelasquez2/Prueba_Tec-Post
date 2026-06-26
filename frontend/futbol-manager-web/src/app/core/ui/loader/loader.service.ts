import { computed, Injectable, signal } from '@angular/core';

/**
 * Servicio global de indicador de carga.
 *
 * Mantiene un contador de operaciones HTTP en curso. El loader visual se
 * muestra mientras el contador es > 0, lo que evita parpadeos cuando varias
 * llamadas se solapan.
 *
 * El {@link loaderInterceptor} llama a `start()` antes de cada request y a
 * `stop()` en su finalize, sin importar éxito o error.
 */
@Injectable({ providedIn: 'root' })
export class LoaderService {
  /** Contador interno de operaciones HTTP activas. */
  private readonly _count = signal(0);

  /** `true` si hay al menos una operación HTTP activa. */
  readonly isLoading = computed(() => this._count() > 0);

  /** Incrementa el contador (una nueva operación arrancó). */
  start(): void { this._count.update(n => n + 1); }

  /** Decrementa el contador. Nunca baja de 0 por defensa. */
  stop(): void { this._count.update(n => Math.max(0, n - 1)); }
}
