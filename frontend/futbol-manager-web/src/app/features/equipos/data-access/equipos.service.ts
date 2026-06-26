import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ActualizarEquipoRequest, CrearEquipoRequest, Equipo } from '../models/equipo.model';

/**
 * Cliente HTTP del recurso `/api/teams`.
 *
 * Cada método es un thin-wrapper sobre {@link HttpClient}. La lógica de
 * estado (cacheo, refresco) vive en los componentes para no acoplar este
 * servicio a una forma concreta de UI.
 */
@Injectable({ providedIn: 'root' })
export class EquiposService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/teams`;

  /**
   * Lista todos los equipos ordenados por nombre.
   * @returns Observable que emite una sola vez con la colección completa.
   */
  listar(): Observable<Equipo[]> {
    return this.http.get<Equipo[]>(this.baseUrl);
  }

  /**
   * Obtiene un equipo por su id.
   * @param id Identificador del equipo.
   */
  obtenerPorId(id: number): Observable<Equipo> {
    return this.http.get<Equipo>(`${this.baseUrl}/${id}`);
  }

  /** Crea un nuevo equipo y devuelve el recurso creado. */
  crear(request: CrearEquipoRequest): Observable<Equipo> {
    return this.http.post<Equipo>(this.baseUrl, request);
  }

  /** Actualiza un equipo existente y devuelve el recurso actualizado. */
  actualizar(id: number, request: ActualizarEquipoRequest): Observable<Equipo> {
    return this.http.put<Equipo>(`${this.baseUrl}/${id}`, request);
  }

  /** Elimina un equipo por id. */
  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
