import { useMemo, useState } from "react";
import { Alert, ScrollView } from "react-native";

import { ProductGrid } from "@/components/prototype/ProductGrid";
import { EmptyState } from "@/components/empty-state";
import { Screen, SearchBar } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_PRODUCTS_BY_TAB } from "@/mock/catalog";

const ALL_PRODUCTS = Object.values(MOCK_PRODUCTS_BY_TAB).flat();

export default function SearchScreen() {
  const [query, setQuery] = useState("");

  const results = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (term.length === 0) return ALL_PRODUCTS;
    return ALL_PRODUCTS.filter((p) => p.toLowerCase().includes(term));
  }, [query]);

  return (
    <Screen>
      <TopBar title="Search" subtitle="Text-based product search" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SearchBar value={query} onChangeText={setQuery} placeholder="Search by quality, product name…" />
        {results.length > 0 ? (
          <ProductGrid items={results} onPress={(item) => Alert.alert(item, "Product detail preview — coming soon.")} />
        ) : (
          <EmptyState message="No products match your search." />
        )}
      </ScrollView>
    </Screen>
  );
}
