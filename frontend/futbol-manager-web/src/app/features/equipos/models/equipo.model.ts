/**
 * Modelo de equipo tal como lo expone la API.
 * Coincide con `EquipoDto` del backend.
 */
export interface Equipo {
  readonly id: number;
  readonly nombre: string;
  readonly ciudad: string | null;
  readonly escudoUrl: string | null;
  readonly createdAt: string;
  readonly updatedAt: string;
}

/** Payload para crear un equipo (POST `/api/teams`). */
export interface CrearEquipoRequest {
  readonly nombre: string;
  readonly ciudad: string | null;
  readonly escudoUrl: string | null;
}

/** Payload para actualizar un equipo (PUT `/api/teams/{id}`). */
export type ActualizarEquipoRequest = CrearEquipoRequest;
