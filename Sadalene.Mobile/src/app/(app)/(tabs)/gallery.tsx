import { Alert, ScrollView } from "react-native";

import { ProductGrid } from "@/components/prototype/ProductGrid";
import { Screen, SectionLabel } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_GALLERY_PRODUCTS } from "@/mock/catalog";

export default function GalleryScreen() {
  return (
    <Screen>
      <TopBar title="Gallery" subtitle="Browse the full collection" customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SectionLabel text="All Products" />
        <ProductGrid
          items={MOCK_GALLERY_PRODUCTS}
          onPress={(item) => Alert.alert(item, "Product detail preview — coming soon.")}
        />
      </ScrollView>
    </Screen>
  );
}
