import type { AuthResponse } from "@/types/dto";

// UI-only pass (prototype implementation) — no backend call. Swap this for the real
// src/api/auth.ts verifyOtp() response once the API is wired back in.
export function buildMockAuthResponse(phone: string): AuthResponse {
  return {
    token: "mock-token",
    identityType: "Customer",
    id: 1,
    displayName: "Raj Textiles",
    phone: phone || "9876500000",
  };
}
