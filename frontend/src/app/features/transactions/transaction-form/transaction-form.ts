import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { finalize, map, startWith, switchMap } from 'rxjs';
import { Product } from '../../../core/models/product.model';
import {
  Transaction,
  TransactionType,
  stockEffect,
} from '../../../core/models/transaction.model';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import { TransactionService } from '../../../core/services/transaction.service';
import {
  INTEGER_PATTERN,
  TWO_DECIMALS_PATTERN,
} from '../../../core/validators/form-validators';
import { ApiDatePipe } from '../../../shared/pipes/api-date.pipe';

// Formulario para registrar y editar compras o ventas.
// Aquí está la validación compleja: no permitir vender más stock del disponible.
@Component({
  selector: 'app-transaction-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    ApiDatePipe,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonToggleModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './transaction-form.html',
})
export class TransactionForm implements OnInit {
  private readonly transactionService = inject(TransactionService);
  private readonly productService = inject(ProductService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  // :id de la ruta (edición) y ?productId= (producto preseleccionado al crear).
  readonly id = input<string>();
  readonly productId = input<string>();

  protected readonly isEdit = computed(() => !!this.id());
  protected readonly products = signal<Product[]>([]);
  protected readonly selectedProduct = signal<Product | null>(null);
  protected readonly original = signal<Transaction | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);

  protected readonly form = inject(FormBuilder).group({
    productId: [null as number | null, Validators.required],
    type: [null as TransactionType | null, Validators.required],
    quantity: [
      null as number | null,
      [Validators.required, Validators.min(1), Validators.pattern(INTEGER_PATTERN)],
    ],
    unitPrice: [
      null as number | null,
      [Validators.required, Validators.min(0), Validators.pattern(TWO_DECIMALS_PATTERN)],
    ],
    detail: ['', Validators.maxLength(500)],
  });

  // Valor del formulario como signal, para calcular el total y el stock en vivo.
  // Uso getRawValue porque en edición el producto está deshabilitado.
  private readonly formValue = toSignal(
    this.form.valueChanges.pipe(
      startWith(null),
      map(() => this.form.getRawValue()),
    ),
    { requireSync: true },
  );

  // Stock con el que cuenta esta transacción.
  // Al editar, primero "devuelvo" el efecto de la transacción original:
  // si era una venta de 5, esas 5 unidades vuelven a estar disponibles.
  protected readonly availableStock = computed(() => {
    const product = this.selectedProduct();
    if (!product) {
      return null;
    }
    const original = this.original();
    return original ? product.stock - stockEffect(original.type, original.quantity) : product.stock;
  });

  // Stock que quedaría después de guardar (misma fórmula del delta del backend).
  protected readonly resultingStock = computed(() => {
    const available = this.availableStock();
    const { type, quantity } = this.formValue();
    return available === null || !type || !quantity ? null : available + stockEffect(type, quantity);
  });

  // Solo visual: el total real lo calcula la base de datos.
  protected readonly total = computed(() => {
    const { quantity, unitPrice } = this.formValue();
    return (quantity ?? 0) * (unitPrice ?? 0);
  });

  constructor() {
    this.form.addValidators(this.stockValidator);

    // Al elegir un producto (solo al crear), muestro su stock y sugiero su
    // precio actual. El usuario puede cambiarlo: en una compra el costo
    // al proveedor suele ser distinto del precio de venta.
    this.form.controls.productId.valueChanges.pipe(takeUntilDestroyed()).subscribe(productId => {
      const product = this.products().find(p => p.id === productId) ?? null;
      this.selectedProduct.set(product);
      if (product && !this.isEdit()) {
        this.form.controls.unitPrice.setValue(product.price);
      }
      this.form.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    if (this.isEdit()) {
      this.loadTransaction(Number(this.id()));
    } else {
      this.loadProducts();
    }
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const detail = value.detail?.trim() || null;
    const operation = this.isEdit()
      ? this.transactionService.update(Number(this.id()), {
          type: value.type!,
          quantity: value.quantity!,
          unitPrice: value.unitPrice!,
          detail,
        })
      : this.transactionService.create({
          productId: value.productId!,
          type: value.type!,
          quantity: value.quantity!,
          unitPrice: value.unitPrice!,
          detail,
        });

    this.saving.set(true);
    operation.pipe(finalize(() => this.saving.set(false))).subscribe(() => {
      this.notification.success(
        this.isEdit()
          ? 'Transacción actualizada. El stock se recalculó.'
          : 'Transacción registrada. El stock se actualizó.',
      );
      this.router.navigate(['/transactions']);
    });
    // Si el backend responde 409 (por ejemplo, otro usuario vendió antes),
    // el interceptor muestra el mensaje y el usuario sigue en el formulario.
  }

  // Validación compleja a nivel de formulario: el stock resultante no puede
  // quedar negativo. Para una venta, significa no vender más de lo disponible.
  // El backend vuelve a validarlo de forma atómica; esto es para avisar antes.
  private readonly stockValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
    const available = this.availableStock();
    const { type, quantity } = group.getRawValue();
    if (available === null || !type || !quantity) {
      return null;
    }
    return available + stockEffect(type, quantity) < 0 ? { insufficientStock: { available } } : null;
  };

  private loadProducts(): void {
    this.loading.set(true);
    this.productService
      .getAllForSelect()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe(products => {
        this.products.set(products);
        // Si llegué desde un producto (?productId=3), lo dejo seleccionado.
        const preselected = Number(this.productId());
        if (preselected && products.some(p => p.id === preselected)) {
          this.form.controls.productId.setValue(preselected);
        }
      });
  }

  private loadTransaction(id: number): void {
    this.loading.set(true);
    this.transactionService
      .getById(id)
      .pipe(
        switchMap(transaction => {
          this.original.set(transaction);
          this.form.patchValue({
            productId: transaction.productId,
            type: transaction.type,
            quantity: transaction.quantity,
            unitPrice: transaction.unitPrice,
            detail: transaction.detail ?? '',
          });
          // El producto no se puede cambiar al editar.
          this.form.controls.productId.disable();
          // Necesito el stock actual del producto para validar.
          return this.productService.getById(transaction.productId);
        }),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: product => {
          this.selectedProduct.set(product);
          this.form.updateValueAndValidity();
        },
        error: () => this.router.navigate(['/transactions']),
      });
  }
}
