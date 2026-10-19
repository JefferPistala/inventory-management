import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Product } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { ApiDatePipe } from '../../../shared/pipes/api-date.pipe';

// Consulta de solo lectura con toda la información de un producto.
@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe, ApiDatePipe, MatButtonModule, MatIconModule, MatProgressBarModule],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly product = signal<Product | null>(null);
  protected readonly loading = signal(false);
  protected readonly imageFailed = signal(false);

  ngOnInit(): void {
    this.loading.set(true);
    this.productService
      .getById(Number(this.id()))
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: product => this.product.set(product),
        error: () => this.router.navigate(['/products']),
      });
  }
}
