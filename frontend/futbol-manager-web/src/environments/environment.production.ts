/**
 * Configuración de entorno para producción.
 * `apiUrl` se inyecta desde variables de entorno del build (o se ajusta aquí).
 */
export const environment = {
  production: true,
  apiUrl: '/api',
} as const;
