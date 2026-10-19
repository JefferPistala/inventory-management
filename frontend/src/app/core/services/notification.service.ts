import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

// Muestro los mensajes de éxito y error en un solo lugar, con el mismo
// estilo en toda la aplicación (las clases están en styles.scss).
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, 'Cerrar', { duration: 3500, panelClass: 'snackbar-success' });
  }

  // Los errores duran más en pantalla para que el usuario alcance a leerlos.
  error(message: string): void {
    this.snackBar.open(message, 'Cerrar', { duration: 6000, panelClass: 'snackbar-error' });
  }
}
