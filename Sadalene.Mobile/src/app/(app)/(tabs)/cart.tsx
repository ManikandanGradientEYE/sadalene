import { useRouter } from "expo-router";
import { ActivityIndicator, Alert, FlatList, Pressable, StyleSheet, Text, View } from "react-native";

import { CartItemRow } from "@/components/cart-item-row";
import { EmptyState } from "@/components/empty-state";
import { ErrorBanner } from "@/components/error-banner";
import { Spacing } from "@/constants/theme";
import { useCart, useClearCart, useRemoveCartItem, useUpdateCartItem } from "@/hooks/use-cart-queries";
import { useEffectiveCustomerId } from "@/hooks/use-effective-customer-id";
import { errorMessage } from "@/lib/queryClient";

export default function CartScreen() {
  const customerId = useEffectiveCustomerId();
  const cart = useCart(customerId);
  const updateItem = useUpdateCartItem(customerId);
  const removeItem = useRemoveCartItem(customerId);
  const clearCart = useClearCart(customerId);
  const router = useRouter();

  if (cart.isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (cart.error) {
    return (
      <View style={styles.center}>
        <ErrorBanner message={errorMessage(cart.error)} />
      </View>
    );
  }

  const items = cart.data?.items ?? [];

  function confirmClear() {
    Alert.alert("Clear cart?", "This removes every item from your cart.", [
      { text: "Cancel", style: "cancel" },
      { text: "Clear", style: "destructive", onPress: () => clearCart.mutate() },
    ]);
  }

  return (
    <View style={styles.container}>
      {cart.data?.divisionName ? (
        <View style={styles.divisionBanner}>
          <Text style={styles.divisionBannerText}>Division: {cart.data.divisionName}</Text>
        </View>
      ) : null}

      <FlatList
        data={items}
        keyExtractor={(item) => String(item.id)}
        renderItem={({ item }) => (
          <CartItemRow
            item={item}
            onChangeQuantity={(quantity) =>
              updateItem.mutate({ cartItemId: item.id, request: { quantity, unitType: item.unitType } })
            }
            onRemove={() => removeItem.mutate(item.id)}
          />
        )}
        ListEmptyComponent={
          <EmptyState message="Your cart is empty. Browse the catalog or scan a product to add items." />
        }
      />

      {items.length > 0 ? (
        <View style={styles.footer}>
          <View style={styles.totalRow}>
            <Text style={styles.totalLabel}>Grand Total</Text>
            <Text style={styles.totalValue}>₹{cart.data?.grandTotal.toFixed(2)}</Text>
          </View>
          <View style={styles.footerButtons}>
            <Pressable style={styles.clearButton} onPress={confirmClear}>
              <Text style={styles.clearButtonText}>Clear</Text>
            </Pressable>
            <Pressable style={styles.checkoutButton} onPress={() => router.push("/checkout")}>
              <Text style={styles.checkoutButtonText}>Checkout</Text>
            </Pressable>
          </View>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, alignItems: "center", justifyContent: "center" },
  divisionBanner: { backgroundColor: "#E1EDFB", padding: Spacing.two, alignItems: "center" },
  divisionBannerText: { fontSize: 13, color: "#1B5FA8", fontWeight: "600" },
  footer: {
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: "#E0E1E6",
    padding: Spacing.three,
    gap: Spacing.two,
  },
  totalRow: { flexDirection: "row", justifyContent: "space-between" },
  totalLabel: { fontSize: 16, fontWeight: "600" },
  totalValue: { fontSize: 18, fontWeight: "700" },
  footerButtons: { flexDirection: "row", gap: Spacing.two },
  clearButton: {
    flex: 1,
    borderWidth: 1,
    borderColor: "#B3261E",
    borderRadius: 10,
    paddingVertical: 12,
    alignItems: "center",
  },
  clearButtonText: { color: "#B3261E", fontWeight: "700" },
  checkoutButton: { flex: 2, backgroundColor: "#208AEF", borderRadius: 10, paddingVertical: 12, alignItems: "center" },
  checkoutButtonText: { color: "#fff", fontWeight: "700" },
});
