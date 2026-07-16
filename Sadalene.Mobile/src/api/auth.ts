import { apiRequest } from "@/api/client";
import type { AuthResponse } from "@/types/dto";

export function sendOtp(phone: string): Promise<{ message: string }> {
  return apiRequest("/auth/send-otp", { method: "POST", body: { phone } });
}

export function verifyOtp(phone: string, code: string): Promise<AuthResponse> {
  return apiRequest("/auth/verify-otp", { method: "POST", body: { phone, code } });
}
