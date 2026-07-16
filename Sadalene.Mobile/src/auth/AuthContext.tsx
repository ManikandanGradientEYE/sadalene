import * as SecureStore from "expo-secure-store";
import React, { createContext, useEffect, useState } from "react";

import { setAuthToken, setUnauthorizedHandler } from "@/api/client";
import type { AuthResponse, IdentityType } from "@/types/dto";

const STORAGE_KEY = "sadalene.auth";

export interface AuthState {
  token: string;
  identityType: IdentityType;
  id: number;
  displayName: string;
  phone: string;
}

export interface AuthContextValue {
  auth: AuthState | null;
  isLoading: boolean;
  signIn: (response: AuthResponse) => Promise<void>;
  signOut: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [auth, setAuth] = useState<AuthState | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  async function signOut() {
    setAuth(null);
    setAuthToken(null);
    await SecureStore.deleteItemAsync(STORAGE_KEY);
  }

  async function signIn(response: AuthResponse) {
    const state: AuthState = {
      token: response.token,
      identityType: response.identityType,
      id: response.id,
      displayName: response.displayName,
      phone: response.phone,
    };
    setAuth(state);
    setAuthToken(state.token);
    await SecureStore.setItemAsync(STORAGE_KEY, JSON.stringify(state));
  }

  // Registered once — a real 401 means the token itself is invalid/expired, so always sign out.
  useEffect(() => {
    setUnauthorizedHandler(() => {
      signOut();
    });
    return () => setUnauthorizedHandler(null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    (async () => {
      try {
        const stored = await SecureStore.getItemAsync(STORAGE_KEY);
        if (stored) {
          const parsed: AuthState = JSON.parse(stored);
          setAuth(parsed);
          setAuthToken(parsed.token);
        }
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  return (
    <AuthContext.Provider value={{ auth, isLoading, signIn, signOut }}>{children}</AuthContext.Provider>
  );
}
