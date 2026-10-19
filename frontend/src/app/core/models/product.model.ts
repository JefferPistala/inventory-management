// Lo que devuelve Products.Api (ProductResponse).
export interface Product {
  id: number;
  name: string;
  description: string | null;
  categoryId: number;
  categoryName: string;
  imageUrl: string | null;
  price: number;
  stock: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

// Lo que envío al crear o editar. No incluye stock: solo cambia con transacciones.
export interface ProductRequest {
  name: string;
  description: string | null;
  categoryId: number;
  imageUrl: string | null;
  price: number;
}

// Filtros dinámicos del listado; los que van vacíos no se envían.
export interface ProductFilters {
  name?: string | null;
  categoryId?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  page: number;
  pageSize: number;
}
