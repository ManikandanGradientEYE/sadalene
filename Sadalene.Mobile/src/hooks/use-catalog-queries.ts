import { useQuery } from "@tanstack/react-query";

import * as catalogApi from "@/api/catalog";

export function useDivisions() {
  return useQuery({ queryKey: ["divisions"], queryFn: catalogApi.getDivisions });
}

export function useCategories(divisionId: number | null) {
  return useQuery({
    queryKey: ["categories", divisionId],
    queryFn: () => catalogApi.getCategories(divisionId!),
    enabled: divisionId != null,
  });
}

export function useSubCategories(categoryId: number | null) {
  return useQuery({
    queryKey: ["subcategories", categoryId],
    queryFn: () => catalogApi.getSubCategories(categoryId!),
    enabled: categoryId != null,
  });
}

export function useProducts(query: catalogApi.ProductQuery) {
  return useQuery({
    queryKey: ["products", query],
    queryFn: () => catalogApi.getProducts(query),
  });
}

export function useProduct(id: number | null) {
  return useQuery({
    queryKey: ["product", id],
    queryFn: () => catalogApi.getProduct(id!),
    enabled: id != null,
  });
}

export function useProductBySku(sku: string | null) {
  return useQuery({
    queryKey: ["product-by-sku", sku],
    queryFn: () => catalogApi.getProductBySku(sku!),
    enabled: sku != null && sku.length > 0,
    retry: false,
  });
}
