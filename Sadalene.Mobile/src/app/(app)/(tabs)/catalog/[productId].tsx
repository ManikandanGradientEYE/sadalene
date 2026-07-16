import { useLocalSearchParams, useRouter } from "expo-router";
import { useState } from "react";
import { ActivityIndicator, Alert, Image, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { ErrorBanner } from "@/components/error-banner";
import { QuantityStepper } from "@/components/quantity-stepper";
import { Spacing } from "@/constants/theme";
import { useAddCartItem } from "@/hooks/use-cart-queries";
import { useProduct } from "@/hooks/use-catalog-queries";
import { useEffectiveCustomerId } from "@/hooks/use-effective-customer-id";
import { ApiError } from "@/lib/apiError";
import { errorMessage } from "@/lib/queryClient";
import type { UnitType } from "@/types/dto";

export default function ProductDetailScreen() {
  const { productId } = useLocalSearchParams<{ productId: string }>();
  const id = Number(productId);
  const router = useRouter();
  const customerId = useEffectiveCustomerId();

  const product = useProduct(Number.isFinite(id) ? id : null);
  const addItem = useAddCartItem(customerId);

  const [quantity, setQuantity] = useState(1);
  const [unitType, setUnitType] = useState<UnitType>("Full");
  const [error, setError] = useState<string | null>(null);

  if (product.isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (product.error || !product.data) {
    return (
      <View style={styles.center}>
        <ErrorBanner message={product.error ? errorMessage(product.error) : "Product not found."} />
      </View>
    );
  }

  const p = product.data;

  async function handleAddToCart() {
    setError(null);
    try {
      await addItem.mutateAsync({ productId: p.id, quantity, unitType });
      Alert.alert("Added to cart", `${p.name} added to your cart.`, [
        { text: "Keep browsing", style: "cancel" },
        { text: "View cart", onPress: () => router.push("/cart") },
      ]);
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        Alert.alert("Different Division", e.message, [
          { text: "OK", style: "cancel" },
          { text: "View Cart", onPress: () => router.push("/cart") },
        ]);
        return;
      }
      setError(errorMessage(e));
    }
  }

  return (
    <ScrollView contentContainerStyle={styles.container}>
      {p.imageUrls.length > 0 ? (
        <Image source={{ uri: p.imageUrls[0] }} style={styles.image} />
      ) : (
        <View style={[styles.image, styles.imagePlaceholder]} />
      )}

      <Text style={styles.name}>{p.name}</Text>
      {p.productCode ? <Text style={styles.code}>{p.productCode}</Text> : null}
      <Text style={styles.division}>
        {p.divisionName} · {p.categoryName} · {p.subCategoryName}
      </Text>

      <View style={styles.priceRow}>
        <Text style={styles.rate}>
          {p.rate != null ? `₹${p.rate.toFixed(2)}` : "—"} / {p.uom}
        </Text>
        <Text style={[styles.stock, p.stock <= 0 && styles.outOfStock]}>
          {p.stock <= 0 ? "Out of stock" : `${p.stock} ${p.uom} available`}
        </Text>
      </View>

      {p.description ? <Text style={styles.description}>{p.description}</Text> : null}

      <View style={styles.specs}>
        {p.fabricComposition ? <Text style={styles.spec}>Composition: {p.fabricComposition}</Text> : null}
        {p.width ? <Text style={styles.spec}>Width: {p.width}</Text> : null}
        {p.weight ? <Text style={styles.spec}>Weight: {p.weight}</Text> : null}
        {p.color ? <Text style={styles.spec}>Color: {p.color}</Text> : null}
        {p.brand ? <Text style={styles.spec}>Brand: {p.brand}</Text> : null}
      </View>

      {p.allowsHalfUnit ? (
        <View style={styles.unitToggle}>
          <Pressable
            style={[styles.unitButton, unitType === "Full" && styles.unitButtonActive]}
            onPress={() => setUnitType("Full")}
          >
            <Text style={[styles.unitButtonText, unitType === "Full" && styles.unitButtonTextActive]}>Full</Text>
          </Pressable>
          <Pressable
            style={[styles.unitButton, unitType === "Half" && styles.unitButtonActive]}
            onPress={() => setUnitType("Half")}
          >
            <Text style={[styles.unitButtonText, unitType === "Half" && styles.unitButtonTextActive]}>Half</Text>
          </Pressable>
        </View>
      ) : null}

      <View style={styles.quantityRow}>
        <Text style={styles.quantityLabel}>Quantity</Text>
        <QuantityStepper value={quantity} onChange={setQuantity} />
      </View>

      {error ? <ErrorBanner message={error} /> : null}

      <Pressable
        style={[styles.addButton, (p.stock <= 0 || addItem.isPending) && styles.addButtonDisabled]}
        onPress={handleAddToCart}
        disabled={p.stock <= 0 || addItem.isPending}
      >
        {addItem.isPending ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.addButtonText}>{p.stock <= 0 ? "Out of Stock" : "Add to Cart"}</Text>
        )}
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { padding: Spacing.three, gap: Spacing.two },
  center: { flex: 1, alignItems: "center", justifyContent: "center" },
  image: { width: "100%", height: 240, borderRadius: 12 },
  imagePlaceholder: { backgroundColor: "#F0F0F3" },
  name: { fontSize: 20, fontWeight: "700" },
  code: { fontSize: 13, color: "#8A8F98" },
  division: { fontSize: 13, color: "#8A8F98" },
  priceRow: { flexDirection: "row", justifyContent: "space-between", marginTop: Spacing.one },
  rate: { fontSize: 18, fontWeight: "700" },
  stock: { fontSize: 13, color: "#1E7A34", alignSelf: "center" },
  outOfStock: { color: "#B3261E" },
  description: { fontSize: 14, color: "#3C3F44", marginTop: Spacing.one },
  specs: { gap: 2, marginTop: Spacing.one },
  spec: { fontSize: 13, color: "#60646C" },
  unitToggle: { flexDirection: "row", gap: Spacing.one, marginTop: Spacing.two },
  unitButton: {
    flex: 1,
    paddingVertical: 10,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#E0E1E6",
    alignItems: "center",
  },
  unitButtonActive: { backgroundColor: "#208AEF", borderColor: "#208AEF" },
  unitButtonText: { fontWeight: "600", color: "#3C3F44" },
  unitButtonTextActive: { color: "#fff" },
  quantityRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: Spacing.two,
  },
  quantityLabel: { fontSize: 15, fontWeight: "600" },
  addButton: {
    backgroundColor: "#208AEF",
    borderRadius: 10,
    paddingVertical: 14,
    alignItems: "center",
    marginTop: Spacing.three,
  },
  addButtonDisabled: { opacity: 0.5 },
  addButtonText: { color: "#fff", fontSize: 16, fontWeight: "700" },
});
