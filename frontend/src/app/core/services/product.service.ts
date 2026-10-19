import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result.model';
import { Product, ProductFilters, ProductRequest } from '../models/product.model';
import { toHttpParams } from '../utils/http-params';

// Un servicio por recurso, igual que un controller por recurso en el backend.
// Aquí solo hago las llamadas HTTP; los errores los maneja el interceptor.
@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.productsApiUrl}/products`;

  getPaged(filters: ProductFilters): Observable<PagedResult<Product>> {
    return this.http.get<PagedResult<Product>>(this.baseUrl, { params: toHttpParams(filters) });
  }

  // Para los selects de producto uso la página máxima que permite la API (100).
  getAllForSelect(): Observable<Product[]> {
    return this.getPaged({ page: 1, pageSize: 100 }).pipe(map(result => result.items));
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  create(request: ProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, request);
  }

  update(id: number, request: ProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
