import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './app.html',
})
export class App {
  constructor() {
    // index.html carga Material Symbols; mat-icon usa Material Icons por defecto.
    inject(MatIconRegistry).setDefaultFontSetClass('material-symbols-outlined');
  }
}
