import { HttpParams } from '@angular/common/http';

// Convierto un objeto de filtros en query params, omitiendo los vacíos.
// Así el backend solo recibe los filtros que el usuario realmente aplicó.
export function toHttpParams(filters: object): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(filters)) {
    if (value !== null && value !== undefined && value !== '') {
      params = params.set(key, String(value));
    }
  }
  return params;
}
