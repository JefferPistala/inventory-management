import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/problem-details.model';
import { NotificationService } from '../services/notification.service';

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
  // Status 0: la petición no llegó (API apagada o bloqueada por CORS).
  if (error.status === 0) {
    return 'No se pudo conectar con el servidor. Verifique que los servicios estén en ejecución.';
  }

  const problem = error.error as ProblemDetails | null;

  if (problem?.errors) {
    return Object.values(problem.errors).flat().join(' ');
  }

  return problem?.detail ?? problem?.title ?? 'Ocurrió un error inesperado.';
}
