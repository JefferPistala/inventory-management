import { PagedResult } from './paged-result.model';

export type TransactionType = 'Purchase' | 'Sale';

export interface Transaction {
  id: number;
  transactionDate: string;
  type: TransactionType;
  productId: number;
  productName: string;
  productIsActive: boolean;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  detail: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTransactionRequest {
  productId: number;
  type: TransactionType;
  quantity: number;
  unitPrice: number;
  detail: string | null;
}

export interface UpdateTransactionRequest {
  type: TransactionType;
  quantity: number;
  unitPrice: number;
  detail: string | null;
}

export interface TransactionFilters {
  productId?: number | null;
  type?: TransactionType | null;
  from?: string | null;
  to?: string | null;
  page: number;
  pageSize: number;
}

export interface ProductHistory {
  productId: number;
  productName: string;
  currentStock: number;
  isActive: boolean;
  transactions: PagedResult<Transaction>;
}

// Misma regla que GetStockEffect en el backend.
export function stockEffect(type: TransactionType, quantity: number): number {
  return type === 'Purchase' ? quantity : -quantity;
}
