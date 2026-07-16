import { Redirect, Stack } from "expo-router";

import { useAuth } from "@/auth/useAuth";

export default function AppLayout() {
  const { auth } = useAuth();
  if (!auth) return <Redirect href="/login" />;
  return <Stack screenOptions={{ headerShown: false }} />;
}
