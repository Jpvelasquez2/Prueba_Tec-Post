import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ActualizarPartidoRequest,
  CrearPartidoRequest,
  Partido,
  RegistrarResultadoRequest,
} from '../models/partido.model';

/**
 * Cliente HTTP del recurso `/api/matches`.
 */
@Injectable({ providedIn: 'root' })
export class PartidosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/matches`;

  /** Lista todos los partidos (más recientes primero). */
  listar(): Observable<Partido[]> {
    return this.http.get<Partido[]>(this.baseUrl);
  }

  /** Obtiene un partido por id. */
  obtenerPorId(id: number): Observable<Partido> {
    return this.http.get<Partido>(`${this.baseUrl}/${id}`);
  }

  /** Programa un nuevo partido. */
  crear(request: CrearPartidoRequest): Observable<Partido> {
    return this.http.post<Partido>(this.baseUrl, request);
  }

  /** Actualiza datos generales del partido (no su resultado). */
  actualizar(id: number, request: ActualizarPartidoRequest): Observable<Partido> {
    return this.http.put<Partido>(`${this.baseUrl}/${id}`, request);
  }

  /** Elimina un partido. */
  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Registra el resultado del partido. La API cambia automáticamente el
   * estado a `Jugado`.
   */
  registrarResultado(id: number, request: RegistrarResultadoRequest): Observable<Partido> {
    return this.http.post<Partido>(`${this.baseUrl}/${id}/result`, request);
  }
}
