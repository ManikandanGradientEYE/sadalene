import { CameraView, useCameraPermissions } from "expo-camera";
import { useRouter } from "expo-router";
import { useState } from "react";
import { ActivityIndicator, Alert, Pressable, StyleSheet, Text, View } from "react-native";

import { getProductBySku } from "@/api/catalog";
import { Spacing } from "@/constants/theme";
import { useAddCartItem } from "@/hooks/use-cart-queries";
import { useEffectiveCustomerId } from "@/hooks/use-effective-customer-id";
import { ApiError } from "@/lib/apiError";
import { errorMessage } from "@/lib/queryClient";

export default function ScanScreen() {
  const [permission, requestPermission] = useCameraPermissions();
  const [isPaused, setIsPaused] = useState(false);
  const customerId = useEffectiveCustomerId();
  const addItem = useAddCartItem(customerId);
  const router = useRouter();

  async function handleBarcodeScanned({ data }: { data: string }) {
    if (isPaused) return;
    setIsPaused(true);

    const sku = data.trim();
    try {
      const product = await getProductBySku(sku);
      Alert.alert(
        product.name,
        `${product.productCode ?? ""}\n₹${product.rate?.toFixed(2) ?? "—"} / ${product.uom}\nStock: ${product.stock}`,
        [
          { text: "Cancel", style: "cancel", onPress: () => setIsPaused(false) },
          {
            text: "Add to Cart",
            onPress: async () => {
              try {
                await addItem.mutateAsync({
                  productId: product.id,
                  quantity: 1,
                  addedByBarcodeScan: true,
                  scannedBarcodeValue: sku,
                });
                Alert.alert("Added", `${product.name} added to your cart.`);
              } catch (e) {
                if (e instanceof ApiError && e.status === 409) {
                  Alert.alert("Different Division", e.message);
                } else {
                  Alert.alert("Couldn't add to cart", errorMessage(e));
                }
              } finally {
                setIsPaused(false);
              }
            },
          },
        ]
      );
    } catch {
      Alert.alert("Not found", `No product found for SKU "${sku}".`, [
        { text: "OK", onPress: () => setIsPaused(false) },
      ]);
    }
  }

  if (!permission) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (!permission.granted) {
    return (
      <View style={styles.center}>
        <Text style={styles.permissionText}>Camera access is needed to scan product barcodes.</Text>
        <Pressable style={styles.permissionButton} onPress={requestPermission}>
          <Text style={styles.permissionButtonText}>Grant Camera Access</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <CameraView
        style={styles.camera}
        facing="back"
        barcodeScannerSettings={{
          barcodeTypes: ["ean13", "ean8", "code128", "code39", "qr", "upc_a", "upc_e"],
        }}
        onBarcodeScanned={isPaused ? undefined : handleBarcodeScanned}
      />
      <View style={styles.overlay}>
        <Text style={styles.overlayText}>
          {isPaused ? "Processing..." : "Point the camera at a product barcode or QR code"}
        </Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  camera: { flex: 1 },
  center: { flex: 1, alignItems: "center", justifyContent: "center", padding: Spacing.four, gap: Spacing.three },
  permissionText: { fontSize: 15, textAlign: "center", color: "#60646C" },
  permissionButton: { backgroundColor: "#208AEF", borderRadius: 10, paddingVertical: 12, paddingHorizontal: 24 },
  permissionButtonText: { color: "#fff", fontWeight: "700" },
  overlay: {
    position: "absolute",
    bottom: Spacing.four,
    left: Spacing.three,
    right: Spacing.three,
    backgroundColor: "rgba(0,0,0,0.6)",
    borderRadius: 10,
    padding: Spacing.two,
  },
  overlayText: { color: "#fff", textAlign: "center", fontSize: 13 },
});
