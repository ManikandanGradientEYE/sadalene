import { useQuery } from "@tanstack/react-query";

import * as customersApi from "@/api/customers";

export function useMyCustomers(enabled: boolean) {
  return useQuery({
    queryKey: ["customers", "mine"],
    queryFn: customersApi.getMyCustomers,
    enabled,
  });
}

export function useCustomerSearch(query: string, enabled: boolean) {
  return useQuery({
    queryKey: ["customers", "search", query],
    queryFn: () => customersApi.searchCustomers(query),
    enabled: enabled && query.trim().length > 0,
  });
}
