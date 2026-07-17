import { useLocalSearchParams, useRouter } from "expo-router";
import { useState } from "react";
import { ScrollView, StyleSheet, Text, View } from "react-native";

import { OptionRow, PrimaryButton, Screen, SectionLabel } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { Colors, Radius, Spacing } from "@/constants/prototype-theme";
import { MOCK_PRODUCT_OPTIONS } from "@/mock/catalog";
import { useMockCart } from "@/mock/MockCartContext";

export default function ProductDetailScreen() {
  const { name } = useLocalSearchParams<{ name: string }>();
  const productName = decodeURIComponent(name ?? "Product");
  const router = useRouter();
  const { addItem } = useMockCart();
  const [option, setOption] = useState(0);

  function handleAddToCart() {
    addItem(productName, MOCK_PRODUCT_OPTIONS[option]);
    router.push("/scan/qty-entry");
  }

  return (
    <Screen>
      <TopBar title={productName} subtitle="Product detail" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <View style={styles.imageWrap}>
          <View style={styles.image}>
            <Text style={styles.imageText}>Product Image</Text>
          </View>
        </View>
        <SectionLabel text="Options" />
        <OptionRow items={MOCK_PRODUCT_OPTIONS} active={option} onChange={setOption} />
        <PrimaryButton text="Add to Cart" variant="gold" onPress={handleAddToCart} />
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
  imageWrap: { paddingHorizontal: Spacing.three, paddingTop: Spacing.two },
  image: {
    height: 190,
    borderRadius: Radius.lg,
    backgroundColor: Colors.creamDk,
    alignItems: "center",
    justifyContent: "center",
  },
  imageText: { color: Colors.grey, fontSize: 11, letterSpacing: 1, textTransform: "uppercase" },
});
