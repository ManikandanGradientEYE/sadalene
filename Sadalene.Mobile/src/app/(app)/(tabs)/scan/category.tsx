import { useLocalSearchParams, useRouter } from "expo-router";
import { useState } from "react";
import { ScrollView } from "react-native";

import { ProductGrid } from "@/components/prototype/ProductGrid";
import { Screen, SectionLabel, SegmentedTabs } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { CATEGORY_TABS, MOCK_PRODUCTS_BY_TAB } from "@/mock/catalog";

export default function CategoryScreen() {
  const { division } = useLocalSearchParams<{ division?: string }>();
  const router = useRouter();
  const [tab, setTab] = useState(0);
  const activeTab = CATEGORY_TABS[tab];

  return (
    <Screen>
      <TopBar title={division ?? "Division"} subtitle="Category selection" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SectionLabel text="Categories" />
        <SegmentedTabs items={CATEGORY_TABS} active={tab} onChange={setTab} />
        <ProductGrid
          items={MOCK_PRODUCTS_BY_TAB[activeTab]}
          onPress={(name) => router.push(`/scan/product/${encodeURIComponent(name)}`)}
        />
      </ScrollView>
    </Screen>
  );
}
