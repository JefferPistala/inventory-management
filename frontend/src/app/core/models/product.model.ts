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

export interface ProductRequest {
  name: string;
  description: string | null;
  categoryId: number;
  imageUrl: string | null;
  price: number;
}

export interface ProductFilters {
  name?: string | null;
  categoryId?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  page: number;
  pageSize: number;
}
