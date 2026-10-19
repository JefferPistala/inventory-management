import { Pipe, PipeTransform } from '@angular/core';
import { TransactionType } from '../../core/models/transaction.model';

// La API usa 'Purchase' y 'Sale'; en pantalla muestro 'Compra' y 'Venta'.
@Pipe({ name: 'transactionType' })
export class TransactionTypePipe implements PipeTransform {
  transform(type: TransactionType): string {
    return type === 'Purchase' ? 'Compra' : 'Venta';
  }
}
