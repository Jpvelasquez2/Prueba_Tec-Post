import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { routes } from './app.routes';
import { errorInterceptor } from './core/http/error.interceptor';
import { loaderInterceptor } from './core/http/loader.interceptor';

/**
 * Configuración raíz de providers de la SPA.
 *
 * - `provideRouter` con `withComponentInputBinding()` para que parámetros
 *   de ruta lleguen como `@Input` directamente.
 * - `provideHttpClient(withInterceptors([...]))` registra el loader y el
 *   manejador global de errores.
 * - `provideZonelessChangeDetection` (Angular 22): la app vive solo con
 *   signals, sin Zone.js, ganando rendimiento y predictibilidad.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([loaderInterceptor, errorInterceptor])),
  ],
};
