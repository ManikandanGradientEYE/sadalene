import { useLocalSearchParams, useRouter } from "expo-router";
import { useEffect, useState } from "react";
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";

import { sendOtp, verifyOtp } from "@/api/auth";
import { useAuth } from "@/auth/useAuth";
import { ErrorBanner } from "@/components/error-banner";
import { Spacing } from "@/constants/theme";
import { errorMessage } from "@/lib/queryClient";

const RESEND_COOLDOWN_SECONDS = 30;

export default function VerifyScreen() {
  const { phone } = useLocalSearchParams<{ phone: string }>();
  const router = useRouter();
  const { signIn } = useAuth();

  const [code, setCode] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cooldown, setCooldown] = useState(RESEND_COOLDOWN_SECONDS);

  useEffect(() => {
    if (cooldown <= 0) return;
    const timer = setTimeout(() => setCooldown((c) => c - 1), 1000);
    return () => clearTimeout(timer);
  }, [cooldown]);

  async function handleVerify() {
    if (code.trim().length === 0) {
      setError("Enter the code you received.");
      return;
    }
    setError(null);
    setIsSubmitting(true);
    try {
      const response = await verifyOtp(phone, code.trim());
      await signIn(response);
      router.replace(response.identityType === "Customer" ? "/catalog" : "/customer-picker");
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleResend() {
    if (cooldown > 0) return;
    setError(null);
    try {
      await sendOtp(phone);
      setCooldown(RESEND_COOLDOWN_SECONDS);
    } catch (e) {
      setError(errorMessage(e));
    }
  }

  return (
    <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <View style={styles.container}>
        <Text style={styles.title}>Enter code</Text>
        <Text style={styles.subtitle}>We sent a code to {phone}.</Text>
        <Pressable onPress={() => router.back()}>
          <Text style={styles.changeNumber}>Change number</Text>
        </Pressable>

        <TextInput
          style={styles.input}
          placeholder="6-digit code"
          keyboardType="number-pad"
          value={code}
          onChangeText={setCode}
          autoFocus
          maxLength={6}
        />

        {error ? <ErrorBanner message={error} /> : null}

        <Pressable style={styles.button} onPress={handleVerify} disabled={isSubmitting}>
          {isSubmitting ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Verify</Text>}
        </Pressable>

        <Pressable onPress={handleResend} disabled={cooldown > 0} style={styles.resend}>
          <Text style={[styles.resendText, cooldown > 0 && styles.resendTextDisabled]}>
            {cooldown > 0 ? `Resend code in ${cooldown}s` : "Resend code"}
          </Text>
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  container: { flex: 1, justifyContent: "center", paddingHorizontal: Spacing.four, gap: Spacing.three },
  title: { fontSize: 28, fontWeight: "700", textAlign: "center" },
  subtitle: { fontSize: 15, color: "#60646C", textAlign: "center" },
  changeNumber: { fontSize: 13, color: "#208AEF", textAlign: "center", marginBottom: Spacing.two },
  input: {
    borderWidth: 1,
    borderColor: "#E0E1E6",
    borderRadius: 10,
    paddingHorizontal: Spacing.three,
    paddingVertical: 14,
    fontSize: 20,
    letterSpacing: 4,
    textAlign: "center",
  },
  button: {
    backgroundColor: "#208AEF",
    borderRadius: 10,
    paddingVertical: 14,
    alignItems: "center",
  },
  buttonText: { color: "#fff", fontSize: 16, fontWeight: "700" },
  resend: { alignItems: "center", paddingVertical: Spacing.two },
  resendText: { fontSize: 14, color: "#208AEF", fontWeight: "600" },
  resendTextDisabled: { color: "#8A8F98" },
});
