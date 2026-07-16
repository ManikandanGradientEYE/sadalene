import { useState } from "react";
import { ActivityIndicator, FlatList, StyleSheet, View } from "react-native";

import { EmptyState } from "@/components/empty-state";
import { ErrorBanner } from "@/components/error-banner";
import { ProductCard } from "@/components/product-card";
import { ProductFilterBar } from "@/components/product-filter-bar";
import { useProducts } from "@/hooks/use-catalog-queries";
import { errorMessage } from "@/lib/queryClient";

export default function CatalogScreen() {
  const [search, setSearch] = useState("");
  const [divisionId, setDivisionId] = useState<number | null>(null);
  const [categoryId, setCategoryId] = useState<number | null>(null);
  const [subCategoryId, setSubCategoryId] = useState<number | null>(null);

  const products = useProducts({
    search: search.trim() || undefined,
    divisionId: divisionId ?? undefined,
    categoryId: categoryId ?? undefined,
    subCategoryId: subCategoryId ?? undefined,
    page: 1,
    pageSize: 30,
  });

  return (
    <View style={styles.container}>
      <ProductFilterBar
        search={search}
        onSearchChange={setSearch}
        divisionId={divisionId}
        categoryId={categoryId}
        subCategoryId={subCategoryId}
        onChange={(filters) => {
          setDivisionId(filters.divisionId);
          setCategoryId(filters.categoryId);
          setSubCategoryId(filters.subCategoryId);
        }}
      />

      {products.error ? <ErrorBanner message={errorMessage(products.error)} /> : null}
      {products.isLoading ? <ActivityIndicator style={styles.loading} /> : null}

      <FlatList
        data={products.data?.items ?? []}
        keyExtractor={(item) => String(item.id)}
        renderItem={({ item }) => <ProductCard product={item} />}
        ListEmptyComponent={!products.isLoading ? <EmptyState message="No products found." /> : null}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  loading: { marginTop: 24 },
});
