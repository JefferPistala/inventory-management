import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category } from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.productsApiUrl}/categories`;

  // Catálogo fijo para llenar los selects de categoría.
  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.baseUrl);
  }
}
