import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, filter, of, switchMap, tap } from 'rxjs';
import {
  Transaction,
  TransactionFilters,
  TransactionType,
} from '../../../core/models/transaction.model';
import { NotificationService } from '../../../core/services/notification.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { toDateParam } from '../../../core/utils/date-utils';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog';
import { ApiDatePipe } from '../../../shared/pipes/api-date.pipe';
import { TransactionTypePipe } from '../../../shared/pipes/transaction-type.pipe';

// Historial de un producto: muestra su nombre y stock actual, y sus
// transacciones con filtros por tipo y rango de fechas.
@Component({
  selector: 'app-product-history',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    ApiDatePipe,
    TransactionTypePipe,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './product-history.html',
})
export class ProductHistory implements OnInit {
  private readonly transactionService = inject(TransactionService);
  private readonly notification = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly displayedColumns = ['date', 'type', 'quantity', 'unitPrice', 'totalPrice', 'detail', 'actions'];

  protected readonly productName = signal('');
  protected readonly currentStock = signal<number | null>(null);
    protected readonly isActive = signal(true);
  protected readonly transactions = signal<Transaction[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(10);
  protected readonly loading = signal(false);

  protected readonly filtersForm = inject(FormBuilder).group({
    type: [null as TransactionType | null],
    from: [null as Date | null],
    to: [null as Date | null],
  });

  private readonly reload$ = new Subject<void>();

  constructor() {
    this.reload$
      .pipe(
        tap(() => this.loading.set(true)),
        switchMap(() =>
          this.transactionService
            .getHistory(Number(this.id()), this.buildFilters())
            .pipe(catchError(() => of(null))),
        ),
        takeUntilDestroyed(),
      )
      .subscribe(history => {
        this.loading.set(false);
        if (!history) {
          // El producto no existe o fue eliminado: regreso al listado.
          this.router.navigate(['/products']);
          return;
        }
        this.productName.set(history.productName);
        this.currentStock.set(history.currentStock);
        this.isActive.set(history.isActive);
        this.transactions.set(history.transactions.items);
        this.totalCount.set(history.transactions.totalCount);
      });

    this.filtersForm.valueChanges.pipe(debounceTime(300), takeUntilDestroyed()).subscribe(() => {
      this.pageIndex.set(0);
      this.reload$.next();
    });
  }

  // El id llega como input de la ruta, por eso hago la primera carga aquí.
  ngOnInit(): void {
    this.reload$.next();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.reload$.next();
  }

  protected clearFilters(): void {
    this.filtersForm.reset({ type: null, from: null, to: null });
  }

  protected deleteTransaction(transaction: Transaction): void {
    const data: ConfirmDialogData = {
      title: 'Eliminar transacción',
      message: 'Al eliminar esta transacción se revertirá su efecto en el stock del producto. ¿Desea continuar?',
      confirmText: 'Eliminar',
    };

    this.dialog
      .open(ConfirmDialog, { data })
      .afterClosed()
      .pipe(
        filter(confirmed => confirmed === true),
        switchMap(() => this.transactionService.delete(transaction.id)),
      )
      .subscribe(() => {
        this.notification.success('Transacción eliminada y stock actualizado.');
        if (this.transactions().length === 1 && this.pageIndex() > 0) {
          this.pageIndex.update(index => index - 1);
        }
        this.reload$.next();
      });
  }

  private buildFilters(): TransactionFilters {
    const { type, from, to } = this.filtersForm.getRawValue();
    return {
      type,
      from: toDateParam(from),
      to: toDateParam(to),
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
    };
  }
}
