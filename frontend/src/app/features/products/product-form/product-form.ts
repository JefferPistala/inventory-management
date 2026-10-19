import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Category } from '../../../core/models/category.model';
import { Product, ProductRequest } from '../../../core/models/product.model';
import { CategoryService } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import {
  TWO_DECIMALS_PATTERN,
  URL_PATTERN,
  notBlankValidator,
} from '../../../core/validators/form-validators';

// Un solo componente para crear y editar: si la ruta trae :id, es edición.
@Component({
  selector: 'app-product-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './product-form.html',
})
export class ProductForm implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  // Viene del parámetro :id de la ruta (gracias a withComponentInputBinding).
  readonly id = input<string>();

  protected readonly isEdit = computed(() => !!this.id());
  protected readonly categories = signal<Category[]>([]);
  protected readonly product = signal<Product | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);

  // Validaciones de campos vacíos y de formato. Coinciden con las del DTO
  // del backend, así el usuario ve el error antes de enviar.
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150), notBlankValidator]],
    description: ['', Validators.maxLength(500)],
    categoryId: [null as number | null, Validators.required],
    imageUrl: ['', [Validators.maxLength(500), Validators.pattern(URL_PATTERN)]],
    price: [
      null as number | null,
      [Validators.required, Validators.min(0), Validators.pattern(TWO_DECIMALS_PATTERN)],
    ],
  });

  // Vista previa de la imagen mientras el usuario escribe la URL.
  private readonly imageUrl = toSignal(this.form.controls.imageUrl.valueChanges, { initialValue: '' });
  protected readonly failedPreviewUrl = signal<string | null>(null);
  protected readonly imagePreview = computed(() => {
    const url = this.imageUrl().trim();
    return URL_PATTERN.test(url) ? url : null;
  });

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(categories => this.categories.set(categories));

    if (this.isEdit()) {
      this.loadProduct(Number(this.id()));
    }
  }

  protected save(): void {
    // Si hay errores, marco todo como tocado para que se vean los mensajes.
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: ProductRequest = {
      name: value.name.trim(),
      description: value.description.trim() || null,
      categoryId: value.categoryId!,
      imageUrl: value.imageUrl.trim() || null,
      price: value.price!,
    };

    const operation = this.isEdit()
      ? this.productService.update(Number(this.id()), request)
      : this.productService.create(request);

    this.saving.set(true);
    operation.pipe(finalize(() => this.saving.set(false))).subscribe(() => {
      this.notification.success(
        this.isEdit() ? 'Producto actualizado correctamente.' : 'Producto creado correctamente.',
      );
      this.router.navigate(['/products']);
    });
  }

  private loadProduct(id: number): void {
    this.loading.set(true);
    this.productService
      .getById(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: product => {
          this.product.set(product);
          this.form.patchValue({
            name: product.name,
            description: product.description ?? '',
            categoryId: product.categoryId,
            imageUrl: product.imageUrl ?? '',
            price: product.price,
          });
        },
        // Si no existe (404), el interceptor avisa y regreso al listado.
        error: () => this.router.navigate(['/products']),
      });
  }
}
