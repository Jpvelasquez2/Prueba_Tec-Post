import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Posicion } from '../models/posicion.model';

/**
 * Cliente HTTP del recurso `/api/standings`.
 *
 * El backend ya devuelve la tabla ordenada (PTS desc, DG desc, GF desc,
 * Nombre asc) y numerada, por lo que el frontend solo tiene que pintarla.
 */
@Injectable({ providedIn: 'root' })
export class PosicionesService {
  private readonly http = inject(HttpClient);

  /** Obtiene la tabla de posiciones completa. */
  obtener(): Observable<Posicion[]> {
    return this.http.get<Posicion[]>(`${environment.apiUrl}/standings`);
  }
}
