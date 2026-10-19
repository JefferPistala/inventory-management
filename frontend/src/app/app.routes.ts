import { Routes } from '@angular/router';

// Cada pantalla se carga de forma diferida (loadComponent): el navegador
// solo descarga el código de una vista cuando el usuario entra en ella.
// Las rutas fijas ('products/new') van antes que las que tienen
// parámetros ('products/:id') para que no se confundan.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },

  {
    path: 'products',
    title: 'Productos',
    loadComponent: () =>
      import('./features/products/product-list/product-list').then(m => m.ProductList),
  },
  {
    path: 'products/new',
    title: 'Nuevo producto',
    loadComponent: () =>
      import('./features/products/product-form/product-form').then(m => m.ProductForm),
  },
  {
    path: 'products/:id',
    title: 'Detalle del producto',
    loadComponent: () =>
      import('./features/products/product-detail/product-detail').then(m => m.ProductDetail),
  },
  {
    path: 'products/:id/edit',
    title: 'Editar producto',
    loadComponent: () =>
      import('./features/products/product-form/product-form').then(m => m.ProductForm),
  },
  {
    path: 'products/:id/history',
    title: 'Historial del producto',
    loadComponent: () =>
      import('./features/products/product-history/product-history').then(m => m.ProductHistory),
  },

  {
    path: 'transactions',
    title: 'Transacciones',
    loadComponent: () =>
      import('./features/transactions/transaction-list/transaction-list').then(
        m => m.TransactionList,
      ),
  },
  {
    path: 'transactions/new',
    title: 'Nueva transacción',
    loadComponent: () =>
      import('./features/transactions/transaction-form/transaction-form').then(
        m => m.TransactionForm,
      ),
  },
  {
    path: 'transactions/:id/edit',
    title: 'Editar transacción',
    loadComponent: () =>
      import('./features/transactions/transaction-form/transaction-form').then(
        m => m.TransactionForm,
      ),
  },

  // Cualquier ruta desconocida vuelve al listado de productos.
  { path: '**', redirectTo: 'products' },
];
