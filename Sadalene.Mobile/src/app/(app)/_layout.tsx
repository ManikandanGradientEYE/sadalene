import { Redirect, Stack } from "expo-router";

import { useAuth } from "@/auth/useAuth";
import { MockCartProvider } from "@/mock/MockCartContext";

export default function AppLayout() {
  const { auth } = useAuth();
  if (!auth) return <Redirect href="/login" />;
  return (
    <MockCartProvider>
      <Stack screenOptions={{ headerShown: false }} />
    </MockCartProvider>
  );
}
