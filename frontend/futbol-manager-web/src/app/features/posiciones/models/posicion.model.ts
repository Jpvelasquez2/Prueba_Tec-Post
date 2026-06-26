/**
 * Fila de la tabla de posiciones tal como la devuelve la API.
 * Coincide con `PosicionDto` del backend.
 */
export interface Posicion {
  readonly posicion: number;
  readonly equipoId: number;
  readonly equipo: string;
  readonly ciudad: string | null;
  readonly escudoUrl: string | null;
  readonly pj: number;
  readonly pg: number;
  readonly pe: number;
  readonly pp: number;
  readonly gf: number;
  readonly gc: number;
  readonly dg: number;
  readonly pts: number;
}
