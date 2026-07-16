import { apiRequest } from "@/api/client";
import type {
  CategoryDto,
  DivisionDto,
  PagedResult,
  ProductDetailDto,
  ProductListItemDto,
  SubCategoryDto,
} from "@/types/dto";

export function getDivisions(): Promise<DivisionDto[]> {
  return apiRequest("/catalog/divisions");
}

export function getCategories(divisionId: number): Promise<CategoryDto[]> {
  return apiRequest("/catalog/categories", { query: { divisionId } });
}

export function getSubCategories(categoryId: number): Promise<SubCategoryDto[]> {
  return apiRequest("/catalog/subcategories", { query: { categoryId } });
}

export interface ProductQuery {
  divisionId?: number;
  categoryId?: number;
  subCategoryId?: number;
  search?: string;
  page?: number;
  pageSize?: number;
  [key: string]: string | number | boolean | undefined | null;
}

export function getProducts(query: ProductQuery): Promise<PagedResult<ProductListItemDto>> {
  return apiRequest("/catalog/products", { query });
}

export function getProduct(id: number): Promise<ProductDetailDto> {
  return apiRequest(`/catalog/products/${id}`);
}

export function getProductBySku(sku: string): Promise<ProductDetailDto> {
  return apiRequest("/catalog/products/by-sku", { query: { sku } });
}
