/**
 * Forma del cuerpo de error devuelto por el backend.
 *
 * El backend emite dos formatos compatibles:
 *  - `ProblemDetails` desde el {@link GlobalExceptionMiddleware} (RFC 7807).
 *  - `ValidationProblemDetails` cuando falla la validación de DataAnnotations
 *    (incluye un mapa de errores por campo en `errors`).
 *
 * Este tipo unifica ambos.
 */
export interface ApiError {
  /** URI tipo del problema (informativa). */
  readonly type?: string;
  /** Título corto del error. */
  readonly title?: string;
  /** Código de estado HTTP replicado. */
  readonly status?: number;
  /** Mensaje legible para el usuario. */
  readonly detail?: string;
  /** Identificador de la traza para correlación con logs del servidor. */
  readonly traceId?: string;
  /** Errores por campo cuando el origen es validación de modelo. */
  readonly errors?: Record<string, string[]>;
}
