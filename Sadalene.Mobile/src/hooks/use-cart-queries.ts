import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import * as cartApi from "@/api/cart";
import type { AddCartItemRequest, UpdateCartItemRequest } from "@/types/dto";

export function useCart(customerId: number | null) {
  return useQuery({
    queryKey: ["cart", customerId],
    queryFn: () => cartApi.getCart(customerId),
    enabled: customerId != null,
  });
}

export function useAddCartItem(customerId: number | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (request: AddCartItemRequest) => cartApi.addCartItem(customerId, request),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["cart", customerId] }),
  });
}

export function useUpdateCartItem(customerId: number | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ cartItemId, request }: { cartItemId: number; request: UpdateCartItemRequest }) =>
      cartApi.updateCartItem(customerId, cartItemId, request),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["cart", customerId] }),
  });
}

export function useRemoveCartItem(customerId: number | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cartItemId: number) => cartApi.removeCartItem(customerId, cartItemId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["cart", customerId] }),
  });
}

export function useClearCart(customerId: number | null) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => cartApi.clearCart(customerId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["cart", customerId] }),
  });
}
