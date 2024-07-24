export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

export class PaginatednResult<T> {
  items?: T;
  pagination?: Pagination
}
