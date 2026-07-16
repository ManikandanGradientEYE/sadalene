const rawBaseUrl = process.env.EXPO_PUBLIC_API_BASE_URL;

if (!rawBaseUrl) {
  throw new Error(
    "EXPO_PUBLIC_API_BASE_URL is not set. Copy .env.example to .env.local and set it to your " +
      "dev machine's LAN IP, e.g. http://192.168.0.104:5266/api — then restart `expo start`."
  );
}

// Strip a trailing slash so callers can always do `${API_BASE_URL}/catalog/...`.
export const API_BASE_URL = rawBaseUrl.replace(/\/+$/, "");
