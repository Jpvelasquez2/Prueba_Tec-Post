import { Component } from '@angular/core';
import { MainLayoutComponent } from './core/layout/main-layout.component';

/**
 * Componente raíz: hospeda únicamente el {@link MainLayoutComponent},
 * que a su vez contiene el header, el `<router-outlet>` y los componentes
 * UI globales (toasts, confirm, loader).
 */
@Component({
  selector: 'app-root',
  imports: [MainLayoutComponent],
  template: `<app-main-layout />`,
  styles: [],
})
export class App {}
