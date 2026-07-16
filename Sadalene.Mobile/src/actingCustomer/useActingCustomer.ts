import { useContext } from "react";

import { ActingCustomerContext } from "@/actingCustomer/ActingCustomerContext";

export function useActingCustomer() {
  const ctx = useContext(ActingCustomerContext);
  if (!ctx) throw new Error("useActingCustomer must be used within ActingCustomerProvider");
  return ctx;
}
