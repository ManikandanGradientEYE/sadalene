import React, { createContext, useContext, useState } from "react";

// UI-only pass — an in-memory mock cart so the browse -> add -> qty -> order-summary screens hang
// together in the demo. Swap this out for src/hooks/use-cart-queries.ts once the API is wired back in.

export interface MockCartLine {
  id: string;
  product: string;
  option?: string;
  qty: number;
}

interface MockCartContextValue {
  lines: MockCartLine[];
  addItem: (product: string, option?: string, qty?: number) => void;
  updateQty: (id: string, qty: number) => void;
  removeItem: (id: string) => void;
  clear: () => void;
}

const MockCartContext = createContext<MockCartContextValue | undefined>(undefined);

export function MockCartProvider({ children }: { children: React.ReactNode }) {
  const [lines, setLines] = useState<MockCartLine[]>([]);

  function addItem(product: string, option?: string, qty = 1) {
    setLines((prev) => {
      const existing = prev.find((l) => l.product === product && l.option === option);
      if (existing) {
        return prev.map((l) => (l.id === existing.id ? { ...l, qty: l.qty + qty } : l));
      }
      return [...prev, { id: `${product}-${option ?? ""}-${Date.now()}`, product, option, qty }];
    });
  }

  function updateQty(id: string, qty: number) {
    setLines((prev) => prev.map((l) => (l.id === id ? { ...l, qty: Math.max(0, qty) } : l)).filter((l) => l.qty > 0));
  }

  function removeItem(id: string) {
    setLines((prev) => prev.filter((l) => l.id !== id));
  }

  function clear() {
    setLines([]);
  }

  return (
    <MockCartContext.Provider value={{ lines, addItem, updateQty, removeItem, clear }}>
      {children}
    </MockCartContext.Provider>
  );
}

export function useMockCart() {
  const ctx = useContext(MockCartContext);
  if (!ctx) throw new Error("useMockCart must be used within MockCartProvider");
  return ctx;
}
