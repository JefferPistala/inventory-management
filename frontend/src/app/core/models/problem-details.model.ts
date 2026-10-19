// Formato estándar de error (RFC 9457) que devuelven las APIs.
// "errors" solo viene en los 400 de validación automática de ASP.NET.
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
