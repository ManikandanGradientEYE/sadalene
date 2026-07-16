import { useRouter } from "expo-router";
import { useState } from "react";
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";

import { ErrorBanner } from "@/components/error-banner";
import { Spacing } from "@/constants/theme";
import { useCart } from "@/hooks/use-cart-queries";
import { useEffectiveCustomerId } from "@/hooks/use-effective-customer-id";
import { useCheckout } from "@/hooks/use-order-queries";
import { errorMessage } from "@/lib/queryClient";

export default function CheckoutScreen() {
  const customerId = useEffectiveCustomerId();
  const cart = useCart(customerId);
  const checkout = useCheckout(customerId);
  const router = useRouter();
  const [notes, setNotes] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function handleCheckout() {
    setError(null);
    try {
      const order = await checkout.mutateAsync({ notes: notes.trim() || undefined });
      router.replace(`/orders/${order.id}`);
    } catch (e) {
      setError(errorMessage(e));
    }
  }

  const items = cart.data?.items ?? [];

  return (
    <ScrollView contentContainerStyle={styles.container}>
      <Text style={styles.title}>Review Order</Text>

      {items.map((item) => (
        <View key={item.id} style={styles.row}>
          <Text style={styles.itemName} numberOfLines={1}>
            {item.productName} ({item.unitType})
          </Text>
          <Text style={styles.itemQty}>x{item.quantity}</Text>
          <Text style={styles.itemTotal}>₹{item.lineTotal.toFixed(2)}</Text>
        </View>
      ))}

      <View style={styles.totalRow}>
        <Text style={styles.totalLabel}>Grand Total</Text>
        <Text style={styles.totalValue}>₹{cart.data?.grandTotal.toFixed(2) ?? "0.00"}</Text>
      </View>

      <Text style={styles.notesLabel}>Notes (optional)</Text>
      <TextInput
        style={styles.notesInput}
        placeholder="Anything the team should know about this order..."
        value={notes}
        onChangeText={setNotes}
        multiline
        numberOfLines={3}
      />

      {error ? <ErrorBanner message={error} /> : null}

      <Pressable
        style={[styles.button, (checkout.isPending || items.length === 0) && styles.buttonDisabled]}
        onPress={handleCheckout}
        disabled={checkout.isPending || items.length === 0}
      >
        {checkout.isPending ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.buttonText}>Place Order</Text>
        )}
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { padding: Spacing.three, gap: Spacing.two },
  title: { fontSize: 20, fontWeight: "700", marginBottom: Spacing.one },
  row: { flexDirection: "row", justifyContent: "space-between", gap: Spacing.one, paddingVertical: 6 },
  itemName: { flex: 1, fontSize: 14 },
  itemQty: { fontSize: 14, color: "#8A8F98" },
  itemTotal: { fontSize: 14, fontWeight: "600", minWidth: 70, textAlign: "right" },
  totalRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: "#E0E1E6",
    marginTop: Spacing.two,
    paddingTop: Spacing.two,
  },
  totalLabel: { fontSize: 16, fontWeight: "700" },
  totalValue: { fontSize: 18, fontWeight: "700" },
  notesLabel: { fontSize: 14, fontWeight: "600", marginTop: Spacing.three },
  notesInput: {
    borderWidth: 1,
    borderColor: "#E0E1E6",
    borderRadius: 10,
    padding: Spacing.two,
    fontSize: 14,
    minHeight: 80,
    textAlignVertical: "top",
  },
  button: {
    backgroundColor: "#208AEF",
    borderRadius: 10,
    paddingVertical: 14,
    alignItems: "center",
    marginTop: Spacing.three,
  },
  buttonDisabled: { opacity: 0.5 },
  buttonText: { color: "#fff", fontSize: 16, fontWeight: "700" },
});
