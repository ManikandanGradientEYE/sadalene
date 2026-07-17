import { useRouter } from "expo-router";
import { useState } from "react";
import { Alert, ScrollView } from "react-native";

import { buildMockAuthResponse } from "@/auth/mockSignIn";
import { useAuth } from "@/auth/useAuth";
import { Banner, FieldInput, LinkRow, PrimaryButton, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";

export default function LoginScreen() {
  const router = useRouter();
  const { signIn } = useAuth();
  const [phone, setPhone] = useState("");
  const [password, setPassword] = useState("");

  // UI-only pass — mock sign-in, no backend call yet (see src/auth/mockSignIn.ts).
  async function handleLogin() {
    await signIn(buildMockAuthResponse(phone));
    router.replace("/home");
  }

  return (
    <Screen>
      <TopBar title="Login" subtitle="Phone + password / OTP" />
      <ScrollView>
        <Banner text="Welcome to SADALENE Ordering App!!" />
        <FieldInput label="Phone No" value={phone} onChangeText={setPhone} keyboardType="phone-pad" />
        <FieldInput label="Password" value={password} onChangeText={setPassword} secure />
        <PrimaryButton text="Login" variant="gold" onPress={handleLogin} />
        <LinkRow
          left="Forgot password?"
          right="Login with OTP instead"
          onLeft={() => Alert.alert("Forgot Password", "Coming soon.")}
          onRight={() => router.push("/otp")}
        />
      </ScrollView>
    </Screen>
  );
}
