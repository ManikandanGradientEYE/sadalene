import { API_BASE_URL } from "@/constants/config";
import { ApiError } from "@/lib/apiError";

// Set by AuthContext once the user logs in / restores a session, read by every request below.
let authToken: string | null = null;
export function setAuthToken(token: string | null) {
  authToken = token;
}

// Registered once by the root layout. Fired on a real 401 (invalid/expired token) — NOT on 403,
// which means "wrong customerId for this call," a different situation the caller handles itself.
let onUnauthorized: (() => void) | null = null;
export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler;
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  query?: Record<string, string | number | boolean | undefined | null>;
}

// Hermes doesn't reliably provide the WHATWG URL/URLSearchParams globals, so query strings are
// built by hand with encodeURIComponent (a plain ECMAScript global, always available) instead.
function buildUrl(path: string, query?: RequestOptions["query"]): string {
  let url = `${API_BASE_URL}${path}`;
  if (query) {
    const parts: string[] = [];
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null) {
        parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`);
      }
    }
    if (parts.length > 0) url += `?${parts.join("&")}`;
  }
  return url;
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, query } = options;

  let response: Response;
  try {
    response = await fetch(buildUrl(path, query), {
      method,
      headers: {
        "Content-Type": "application/json",
        ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(0, "Couldn't reach the server. Check your connection and the API address in settings.");
  }

  if (response.status === 401) {
    onUnauthorized?.();
  }

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const data = await response.json();
      if (data?.message) message = data.message;
    } catch {
      // Body wasn't JSON (empty, HTML error page, etc.) — keep the generic message.
    }
    throw new ApiError(response.status, message);
  }

  if (response.status === 204) return undefined as T;

  return (await response.json()) as T;
}
