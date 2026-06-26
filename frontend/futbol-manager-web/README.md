# FutbolManager — Frontend (Angular)

SPA en **Angular 22 LTS** con **arquitectura Feature-Based**, TypeScript, **Tailwind CSS 4** y RxJS.

## Estructura

```
futbol-manager-web/
├── src/
│   ├── main.ts
│   ├── index.html
│   ├── styles.css                  # @import "tailwindcss" + tokens
│   └── app/
│       ├── app.ts                  # Componente raíz
│       ├── app.config.ts           # providers (router, http, interceptors)
│       ├── app.routes.ts           # Tabla de rutas raíz
│       ├── core/                   # Singletons (auth, http, layout, config)
│       ├── shared/                 # UI / pipes / directivas / validators
│       └── features/               # equipos, jugadores, partidos, torneos, dashboard
├── .postcssrc.json                 # Activa @tailwindcss/postcss
├── angular.json
├── package.json
└── tsconfig.json
```

## Convenciones

* **Standalone components** y **signals** (Angular 22).
* Carga **lazy** por feature.
* Smart vs Dumb components.
* Comentarios **JSDoc** en todo componente, servicio, pipe y función pública.
* Selectores HTML con prefijo `app-`.
* Archivos en `kebab-case` con sufijo de tipo (`.component.ts`, `.service.ts`, `.guard.ts`).
* Tipos / interfaces sin prefijo `I`.

## Scripts npm

```powershell
npm start          # ng serve
npm run build      # ng build
npm test           # ng test (vitest)
```

## Tailwind

Configurado con Tailwind 4 vía `@tailwindcss/postcss` y `@import "tailwindcss"` en `src/styles.css`.
La configuración es CSS-first (no usa `tailwind.config.js`).

## Requisitos

* Usar las herramientas portables del repo: `..\..\scripts\utils\activate-env.ps1`.

> **Estado:** estructura base lista. Sin features implementadas.
> Ver `ARQUITECTURA.md` en la raíz del repositorio.
