import { apiRequest } from "@/api/client";
import type { CheckoutRequest, OrderDetailDto, OrderSummaryDto, PagedResult } from "@/types/dto";

export function checkout(customerId: number | null, request: CheckoutRequest): Promise<OrderDetailDto> {
  return apiRequest("/orders/checkout", {
    method: "POST",
    body: request,
    query: { customerId: customerId ?? undefined },
  });
}

export function getMyOrders(page: number, pageSize = 20): Promise<PagedResult<OrderSummaryDto>> {
  return apiRequest("/orders/mine", { query: { page, pageSize } });
}

export function getOrder(id: number): Promise<OrderDetailDto> {
  return apiRequest(`/orders/${id}`);
}
