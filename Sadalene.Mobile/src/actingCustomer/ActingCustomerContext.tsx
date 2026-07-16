import React, { createContext, useState } from "react";

export interface ActingCustomer {
  id: number;
  name: string;
}

export interface ActingCustomerContextValue {
  actingCustomer: ActingCustomer | null;
  setActingCustomer: (customer: ActingCustomer | null) => void;
}

export const ActingCustomerContext = createContext<ActingCustomerContextValue | undefined>(undefined);

// Deliberately in-memory only, never persisted to SecureStore — an Agent/Staff member re-picks
// which customer they're serving on every cold start, so a stale pick can't survive a restart.
export function ActingCustomerProvider({ children }: { children: React.ReactNode }) {
  const [actingCustomer, setActingCustomer] = useState<ActingCustomer | null>(null);
  return (
    <ActingCustomerContext.Provider value={{ actingCustomer, setActingCustomer }}>
      {children}
    </ActingCustomerContext.Provider>
  );
}
