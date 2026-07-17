import { useRouter } from "expo-router";
import { useState } from "react";
import { ScrollView } from "react-native";

import { buildMockAuthResponse } from "@/auth/mockSignIn";
import { useAuth } from "@/auth/useAuth";
import { Banner, FieldInput, PrimaryButton, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";

export default function OtpScreen() {
  const router = useRouter();
  const { signIn } = useAuth();
  const [phone, setPhone] = useState("");
  const [code, setCode] = useState("");

  // UI-only pass — mock sign-in, no backend call yet (see src/auth/mockSignIn.ts).
  async function handleVerify() {
    await signIn(buildMockAuthResponse(phone));
    router.replace("/home");
  }

  return (
    <Screen>
      <TopBar title="Login with OTP" subtitle="Phone + one-time code" back />
      <ScrollView>
        <Banner text="Enter your phone number to receive a code" />
        <FieldInput label="Phone No" value={phone} onChangeText={setPhone} keyboardType="phone-pad" />
        <FieldInput label="OTP Code" value={code} onChangeText={setCode} keyboardType="number-pad" placeholder="6-digit code" />
        <PrimaryButton text="Verify & Login" variant="gold" onPress={handleVerify} />
      </ScrollView>
    </Screen>
  );
}
