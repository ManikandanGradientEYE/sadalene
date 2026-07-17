import { Redirect } from "expo-router";

import { useAuth } from "@/auth/useAuth";

// The single source of truth for "where do I land on launch" — everything else guards against
// direct navigation to the wrong place, but this is what resolves the bare "/" on cold start.
export default function Index() {
  const { auth } = useAuth();
  if (!auth) return <Redirect href="/login" />;
  return <Redirect href={auth.identityType === "Customer" ? "/home" : "/customer-picker"} />;
}
