import { useActingCustomer } from "@/actingCustomer/useActingCustomer";
import { useAuth } from "@/auth/useAuth";

// The customerId to pass on every Cart/Orders call: a Customer always acts as themselves; an
// Agent/Staff member acts as whoever they picked in the Customer Picker screen (null until chosen).
export function useEffectiveCustomerId(): number | null {
  const { auth } = useAuth();
  const { actingCustomer } = useActingCustomer();

  if (!auth) return null;
  if (auth.identityType === "Customer") return auth.id;
  return actingCustomer?.id ?? null;
}
