/**
 * Configuración de entorno para desarrollo local.
 * El backend ASP.NET Core levanta por defecto en `http://localhost:5200`.
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5200/api',
} as const;
