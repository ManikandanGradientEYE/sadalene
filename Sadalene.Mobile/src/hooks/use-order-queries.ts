import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import * as ordersApi from "@/api/orders";
import type { CheckoutRequest } from "@/types/dto";

export function useMyOrders() {
  return useQuery({
    queryKey: ["orders", "mine"],
    queryFn: () => ordersApi.getMyOrders(1, 50),
  });
}

export function useOrder(id: number | null) {
  return useQuery({
    queryKey: ["order", id],
    queryFn: () => ordersApi.getOrder(id!),
    enabled: id != null,
  });
}

export function useCheckout(customerId: number | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (request: CheckoutRequest) => ordersApi.checkout(customerId, request),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["cart", customerId] });
      qc.invalidateQueries({ queryKey: ["orders", "mine"] });
    },
  });
}
