import { QueryClient } from "@tanstack/react-query";

import { ApiError } from "@/lib/apiError";

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Don't retry 4xx (client errors, e.g. 404/403/409) — retrying won't change the outcome.
      retry: (failureCount, error) => {
        if (error instanceof ApiError && error.status >= 400 && error.status < 500) return false;
        return failureCount < 2;
      },
    },
    mutations: {
      retry: false,
    },
  },
});

export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message;
  if (error instanceof Error) return error.message;
  return "Something went wrong.";
}
