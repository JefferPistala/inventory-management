// errors solo llega en los 400 de validación automática de ASP.NET.
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
