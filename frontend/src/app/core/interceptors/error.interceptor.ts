import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/problem-details.model';
import { NotificationService } from '../services/notification.service';

// Intercepto TODAS las respuestas con error y muestro el mensaje que envía
// el backend (ProblemDetails). Así no repito el manejo de errores en cada
// componente. Relanzo el error para que el componente pueda reaccionar
// (por ejemplo, quitar el indicador de carga).
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const notification = inject(NotificationService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      notification.error(getErrorMessage(error));
      return throwError(() => error);
    }),
  );
};

function getErrorMessage(error: HttpErrorResponse): string {
  // Status 0: la petición ni siquiera llegó (API apagada o bloqueada por CORS).
  if (error.status === 0) {
    return 'No se pudo conectar con el servidor. Verifique que los servicios estén en ejecución.';
  }

  const problem = error.error as ProblemDetails | null;

  // 400 de validación automática: junto todos los mensajes de los campos.
  if (problem?.errors) {
    return Object.values(problem.errors).flat().join(' ');
  }

  // 404, 409, 503...: el backend ya envía un "detail" pensado para el usuario.
  return problem?.detail ?? problem?.title ?? 'Ocurrió un error inesperado.';
}
