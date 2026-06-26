/**
 * Estados posibles de un partido. Valores numéricos espejados del enum
 * `EstadoPartido` del backend (1=Programado, 2=Jugado, 3=Suspendido, 4=Cancelado).
 */
export enum EstadoPartido {
  Programado = 1,
  Jugado     = 2,
  Suspendido = 3,
  Cancelado  = 4,
}

/**
 * Etiqueta legible por humanos para cada estado.
 * Se usa en tablas, badges y filtros.
 */
export const ETIQUETAS_ESTADO: Readonly<Record<EstadoPartido, string>> = {
  [EstadoPartido.Programado]: 'Programado',
  [EstadoPartido.Jugado]:     'Jugado',
  [EstadoPartido.Suspendido]: 'Suspendido',
  [EstadoPartido.Cancelado]:  'Cancelado',
} as const;

/**
 * Modelo de partido tal como lo expone la API.
 * Coincide con `PartidoDto` del backend.
 */
export interface Partido {
  readonly id: number;
  readonly fecha: string;
  readonly localTeamId: number;
  readonly localTeamNombre: string;
  readonly visitanteTeamId: number;
  readonly visitanteTeamNombre: string;
  readonly localScore: number | null;
  readonly visitanteScore: number | null;
  readonly estado: EstadoPartido;
  readonly createdAt: string;
  readonly updatedAt: string;
}

/** Payload para programar un partido (POST `/api/matches`). */
export interface CrearPartidoRequest {
  readonly fecha: string;
  readonly localTeamId: number;
  readonly visitanteTeamId: number;
  readonly estado?: EstadoPartido;
}

/** Payload para actualizar un partido (PUT `/api/matches/{id}`). */
export interface ActualizarPartidoRequest {
  readonly fecha: string;
  readonly localTeamId: number;
  readonly visitanteTeamId: number;
  readonly estado: EstadoPartido;
}

/** Payload para registrar el resultado (POST `/api/matches/{id}/result`). */
export interface RegistrarResultadoRequest {
  readonly localScore: number;
  readonly visitanteScore: number;
}
