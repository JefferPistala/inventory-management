import { PagedResult } from './paged-result.model';

// La API envía el tipo como texto gracias a JsonStringEnumConverter.
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

// No envío el total: lo calcula la base de datos.
export interface CreateTransactionRequest {
  productId: number;
  type: TransactionType;
  quantity: number;
  unitPrice: number;
  detail: string | null;
}

// Sin productId: el producto de una transacción no se puede cambiar.
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

// Historial: datos actuales del producto + sus transacciones paginadas.
export interface ProductHistory {
  productId: number;
  productName: string;
  currentStock: number;
  isActive: boolean;
  transactions: PagedResult<Transaction>;
}

// Único lugar del frontend donde decido el signo: compra suma, venta resta.
// Es la misma regla que usa el backend (GetStockEffect).
export function stockEffect(type: TransactionType, quantity: number): number {
  return type === 'Purchase' ? quantity : -quantity;
}
