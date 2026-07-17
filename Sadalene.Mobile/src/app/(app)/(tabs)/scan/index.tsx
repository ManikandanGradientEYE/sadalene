import { useRouter } from "expo-router";
import { Alert, ScrollView } from "react-native";

import { CameraBox, NoteBox, PrimaryButton, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { useMockCart } from "@/mock/MockCartContext";

// UI-only pass — the camera box is a static placeholder for now (no real scan/lookup yet); tapping
// it simulates a successful scan by jumping straight to a product detail screen.
export default function ScanScreen() {
  const router = useRouter();
  const { addItem } = useMockCart();

  function handleScanned() {
    router.push("/scan/product/Product%202");
  }

  function handleAddToCart() {
    addItem("Product 2");
    Alert.alert("Added to Cart", "1 pc added to your cart.");
  }

  return (
    <Screen>
      <TopBar title="Scan to Order" subtitle="Camera / barcode capture" customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <CameraBox label="Open Camera" onPress={handleScanned} />
        <NoteBox text="After scanning, the product image is shown here." />
        <PrimaryButton text="Add to Cart" onPress={handleAddToCart} />
      </ScrollView>
    </Screen>
  );
}
