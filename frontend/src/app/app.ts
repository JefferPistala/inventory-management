import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

// Componente raíz: barra de navegación + contenedor donde se cargan las vistas.
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './app.html',
})
export class App {
  constructor() {
    // index.html carga la fuente "Material Symbols". Le indico a <mat-icon>
    // que la use por defecto en lugar de la fuente antigua "Material Icons".
    inject(MatIconRegistry).setDefaultFontSetClass('material-symbols-outlined');
  }
}
