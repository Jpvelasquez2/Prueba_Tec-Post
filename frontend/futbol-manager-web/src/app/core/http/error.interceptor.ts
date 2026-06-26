import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../models/api-error.model';
import { ToastService } from '../ui/toast/toast.service';

/**
 * Interceptor HTTP que captura errores y muestra un toast con el mensaje legible.
 *
 * - Para 400 con `errors` (validación de modelo) muestra el primer error de campo.
 * - Para el resto, prefiere `detail` (ProblemDetails) sobre el mensaje genérico.
 * - Re-lanza siempre el error para que el componente pueda reaccionar si quiere.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((response: HttpErrorResponse) => {
      const mensaje = extraerMensaje(response);
      toast.error(mensaje);
      return throwError(() => response);
    }),
  );
};

/**
 * Deriva un mensaje amigable a partir de la respuesta de error.
 * Centralizado para asegurar el mismo formato en toda la app.
 */
function extraerMensaje(response: HttpErrorResponse): string {
  // Errores de red / CORS / servidor caído.
  if (response.status === 0) {
    return 'No se pudo contactar al servidor. Verifica que la API esté arriba.';
  }

  const body = response.error as ApiError | string | null;

  if (typeof body === 'string' && body) return body;

  if (body && typeof body === 'object') {
    // Validación: priorizar el primer error de campo.
    if (body.errors) {
      const primeraClave = Object.keys(body.errors)[0];
      const primerError = primeraClave ? body.errors[primeraClave]?.[0] : undefined;
      if (primerError) return primerError;
    }
    if (body.detail)  return body.detail;
    if (body.title)   return body.title;
  }

  // Fallback por código.
  return `Error ${response.status} ${response.statusText || ''}`.trim();
}
