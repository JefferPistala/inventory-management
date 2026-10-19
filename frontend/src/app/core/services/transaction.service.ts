import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result.model';
import {
  CreateTransactionRequest,
  ProductHistory,
  Transaction,
  TransactionFilters,
  UpdateTransactionRequest,
} from '../models/transaction.model';
import { toHttpParams } from '../utils/http-params';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.transactionsApiUrl}/transactions`;

  getPaged(filters: TransactionFilters): Observable<PagedResult<Transaction>> {
    return this.http.get<PagedResult<Transaction>>(this.baseUrl, { params: toHttpParams(filters) });
  }

  // El producto va en la ruta; el resto de filtros como query params.
  getHistory(productId: number, filters: TransactionFilters): Observable<ProductHistory> {
    return this.http.get<ProductHistory>(`${this.baseUrl}/history/${productId}`, {
      params: toHttpParams(filters),
    });
  }

  getById(id: number): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateTransactionRequest): Observable<Transaction> {
    return this.http.post<Transaction>(this.baseUrl, request);
  }

  update(id: number, request: UpdateTransactionRequest): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
