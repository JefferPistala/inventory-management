import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, filter, of, switchMap, tap } from 'rxjs';
import { Category } from '../../../core/models/category.model';
import { Product, ProductFilters } from '../../../core/models/product.model';
import { CategoryService } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import { rangeValidator } from '../../../core/validators/form-validators';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-product-list',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
  ],
  templateUrl: './product-list.html',
})
export class ProductList {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly notification = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  protected readonly displayedColumns = ['image', 'name', 'category', 'price', 'stock', 'actions'];

  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<Category[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(10);
  protected readonly loading = signal(false);

  protected readonly filtersForm = inject(FormBuilder).group(
    {
      name: [''],
      categoryId: [null as number | null],
      minPrice: [null as number | null, Validators.min(0)],
      maxPrice: [null as number | null, Validators.min(0)],
    },
    { validators: rangeValidator('minPrice', 'maxPrice') },
  );

  // switchMap cancela la búsqueda anterior si llega una nueva.
  private readonly reload$ = new Subject<void>();

  constructor() {
    this.reload$
      .pipe(
        tap(() => this.loading.set(true)),
        switchMap(() =>
          this.productService.getPaged(this.buildFilters()).pipe(catchError(() => of(null))),
        ),
        takeUntilDestroyed(),
      )
      .subscribe(result => {
        this.loading.set(false);
        if (result) {
          this.products.set(result.items);
          this.totalCount.set(result.totalCount);
        }
      });

    this.filtersForm.valueChanges
      .pipe(
        debounceTime(400),
        filter(() => this.filtersForm.valid),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        this.pageIndex.set(0);
        this.reload$.next();
      });

    this.categoryService.getAll().subscribe(categories => this.categories.set(categories));
    this.reload$.next();
  }

  protected onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.reload$.next();
  }

  protected clearFilters(): void {
    this.filtersForm.reset({ name: '', categoryId: null, minPrice: null, maxPrice: null });
  }

  protected deleteProduct(product: Product): void {
    const data: ConfirmDialogData = {
      title: 'Eliminar producto',
      message: `¿Desea eliminar "${product.name}"? Dejará de aparecer en el sistema, pero su historial de transacciones se conservará.`,
      confirmText: 'Eliminar',
    };

    this.dialog
      .open(ConfirmDialog, { data })
      .afterClosed()
      .pipe(
        filter(confirmed => confirmed === true),
        switchMap(() => this.productService.delete(product.id)),
      )
      .subscribe(() => {
        this.notification.success('Producto eliminado correctamente.');
        // Si eliminé el único registro de la página, retrocedo una página.
        if (this.products().length === 1 && this.pageIndex() > 0) {
          this.pageIndex.update(index => index - 1);
        }
        this.reload$.next();
      });
  }

  protected readonly brokenImages = signal(new Set<number>());

  protected onImageError(productId: number): void {
    this.brokenImages.update(ids => new Set(ids).add(productId));
  }

  private buildFilters(): ProductFilters {
    const { name, categoryId, minPrice, maxPrice } = this.filtersForm.getRawValue();
    return {
      name: name?.trim() || null,
      categoryId,
      minPrice,
      maxPrice,
      page: this.pageIndex() + 1, // el paginador cuenta desde 0; la API desde 1
      pageSize: this.pageSize(),
    };
  }
}
