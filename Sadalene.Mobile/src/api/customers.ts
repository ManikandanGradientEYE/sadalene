import { apiRequest } from "@/api/client";
import type { CustomerListItemDto } from "@/types/dto";

// Agent-only: their own customer list.
export function getMyCustomers(): Promise<CustomerListItemDto[]> {
  return apiRequest("/customers/mine");
}

// Staff-only: search across all customers.
export function searchCustomers(q: string): Promise<CustomerListItemDto[]> {
  return apiRequest("/customers/search", { query: { q } });
}
