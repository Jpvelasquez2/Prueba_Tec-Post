import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoaderService } from '../ui/loader/loader.service';

/**
 * Interceptor HTTP que mantiene el contador del {@link LoaderService}
 * incrementado mientras hay peticiones en vuelo.
 *
 * Usa `finalize` para garantizar el decremento incluso si la petición falla.
 */
export const loaderInterceptor: HttpInterceptorFn = (req, next) => {
  const loader = inject(LoaderService);
  loader.start();
  return next(req).pipe(finalize(() => loader.stop()));
};
