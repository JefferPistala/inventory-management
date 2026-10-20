import { Pipe, PipeTransform } from '@angular/core';
import { TransactionType } from '../../core/models/transaction.model';

@Pipe({ name: 'transactionType' })
export class TransactionTypePipe implements PipeTransform {
  transform(type: TransactionType): string {
    return type === 'Purchase' ? 'Compra' : 'Venta';
  }
}
