import { Redirect, Stack } from "expo-router";

import { useAuth } from "@/auth/useAuth";

export default function AuthLayout() {
  const { auth } = useAuth();
  if (auth) {
    return <Redirect href={auth.identityType === "Customer" ? "/home" : "/customer-picker"} />;
  }
  return <Stack screenOptions={{ headerShown: false }} />;
}
