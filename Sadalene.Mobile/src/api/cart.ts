import { apiRequest } from "@/api/client";
import type { AddCartItemRequest, CartDto, UpdateCartItemRequest } from "@/types/dto";

export function getCart(customerId: number | null): Promise<CartDto> {
  return apiRequest("/cart", { query: { customerId: customerId ?? undefined } });
}

export function addCartItem(customerId: number | null, request: AddCartItemRequest): Promise<CartDto> {
  return apiRequest("/cart/items", {
    method: "POST",
    body: request,
    query: { customerId: customerId ?? undefined },
  });
}

export function updateCartItem(
  customerId: number | null,
  cartItemId: number,
  request: UpdateCartItemRequest
): Promise<CartDto> {
  return apiRequest(`/cart/items/${cartItemId}`, {
    method: "PUT",
    body: request,
    query: { customerId: customerId ?? undefined },
  });
}

export function removeCartItem(customerId: number | null, cartItemId: number): Promise<CartDto> {
  return apiRequest(`/cart/items/${cartItemId}`, {
    method: "DELETE",
    query: { customerId: customerId ?? undefined },
  });
}

export function clearCart(customerId: number | null): Promise<CartDto> {
  return apiRequest("/cart", { method: "DELETE", query: { customerId: customerId ?? undefined } });
}
