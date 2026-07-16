import { useRouter } from "expo-router";
import { useState } from "react";
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

import { sendOtp } from "@/api/auth";
import { ErrorBanner } from "@/components/error-banner";
import { Spacing } from "@/constants/theme";
import { errorMessage } from "@/lib/queryClient";

export default function LoginScreen() {
  const router = useRouter();
  const [phone, setPhone] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit() {
    if (phone.trim().length === 0) {
      setError("Enter your phone number.");
      return;
    }
    setError(null);
    setIsSubmitting(true);
    try {
      await sendOtp(phone.trim());
      router.push({ pathname: "/verify", params: { phone: phone.trim() } });
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <View style={styles.container}>
        <Text style={styles.title}>Sadalene</Text>
        <Text style={styles.subtitle}>Enter your phone number to sign in.</Text>

        <TextInput
          style={styles.input}
          placeholder="Phone number"
          keyboardType="phone-pad"
          value={phone}
          onChangeText={setPhone}
          autoFocus
        />

        {error ? <ErrorBanner message={error} /> : null}

        <Pressable style={styles.button} onPress={handleSubmit} disabled={isSubmitting}>
          {isSubmitting ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Send OTP</Text>}
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  container: { flex: 1, justifyContent: "center", paddingHorizontal: Spacing.four, gap: Spacing.three },
  title: { fontSize: 32, fontWeight: "700", textAlign: "center" },
  subtitle: { fontSize: 15, color: "#60646C", textAlign: "center", marginBottom: Spacing.two },
  input: {
    borderWidth: 1,
    borderColor: "#E0E1E6",
    borderRadius: 10,
    paddingHorizontal: Spacing.three,
    paddingVertical: 14,
    fontSize: 16,
  },
  button: {
    backgroundColor: "#208AEF",
    borderRadius: 10,
    paddingVertical: 14,
    alignItems: "center",
  },
  buttonText: { color: "#fff", fontSize: 16, fontWeight: "700" },
});
