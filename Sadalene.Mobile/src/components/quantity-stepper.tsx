import { Pressable, StyleSheet, Text, TextInput, View } from "react-native";

import { Spacing } from "@/constants/theme";

interface Props {
  value: number;
  onChange: (value: number) => void;
  min?: number;
}

// Quantity is a decimal server-side (fabric quantities can be fractional), so this is a text input
// that accepts decimals, not an integer-only +/- stepper.
export function QuantityStepper({ value, onChange, min = 0.001 }: Props) {
  function step(delta: number) {
    const next = Math.max(min, Math.round((value + delta) * 1000) / 1000);
    onChange(next);
  }

  return (
    <View style={styles.container}>
      <Pressable style={styles.button} onPress={() => step(-1)} hitSlop={6}>
        <Text style={styles.buttonText}>−</Text>
      </Pressable>
      <TextInput
        style={styles.input}
        keyboardType="decimal-pad"
        value={String(value)}
        onChangeText={(text) => {
          const parsed = parseFloat(text);
          if (!Number.isNaN(parsed)) onChange(parsed);
        }}
      />
      <Pressable style={styles.button} onPress={() => step(1)} hitSlop={6}>
        <Text style={styles.buttonText}>+</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flexDirection: "row", alignItems: "center", gap: Spacing.one },
  button: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: "#F0F0F3",
    alignItems: "center",
    justifyContent: "center",
  },
  buttonText: { fontSize: 18, fontWeight: "600" },
  input: {
    minWidth: 48,
    textAlign: "center",
    fontSize: 16,
    fontWeight: "600",
    paddingVertical: 4,
  },
});
