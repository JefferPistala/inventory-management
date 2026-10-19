// Misma forma que el PagedResult<T> que devuelven ambas APIs.
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
