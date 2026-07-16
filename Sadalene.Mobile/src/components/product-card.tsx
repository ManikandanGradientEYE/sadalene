import { Link } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";

import { Spacing } from "@/constants/theme";
import type { ProductListItemDto } from "@/types/dto";

export function ProductCard({ product }: { product: ProductListItemDto }) {
  return (
    <Link href={`/catalog/${product.id}`} asChild>
      <Pressable style={styles.card}>
        {product.primaryImageUrl ? (
          <Image source={{ uri: product.primaryImageUrl }} style={styles.image} />
        ) : (
          <View style={[styles.image, styles.imagePlaceholder]} />
        )}
        <View style={styles.info}>
          <Text style={styles.name} numberOfLines={1}>
            {product.name}
          </Text>
          {product.productCode ? <Text style={styles.code}>{product.productCode}</Text> : null}
          <Text style={styles.division}>{product.divisionName}</Text>
          <View style={styles.row}>
            <Text style={styles.rate}>{product.rate != null ? `₹${product.rate.toFixed(2)}` : "—"}</Text>
            <Text style={[styles.stock, product.stock <= 0 && styles.outOfStock]}>
              {product.stock <= 0 ? "Out of stock" : `${product.stock} ${product.uom}`}
            </Text>
          </View>
        </View>
      </Pressable>
    </Link>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: "row",
    gap: Spacing.two,
    paddingVertical: Spacing.two,
    paddingHorizontal: Spacing.three,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: "#E0E1E6",
  },
  image: { width: 56, height: 56, borderRadius: 8 },
  imagePlaceholder: { backgroundColor: "#F0F0F3" },
  info: { flex: 1, gap: 2 },
  name: { fontSize: 15, fontWeight: "600" },
  code: { fontSize: 12, color: "#8A8F98" },
  division: { fontSize: 12, color: "#8A8F98" },
  row: { flexDirection: "row", justifyContent: "space-between", marginTop: 4 },
  rate: { fontSize: 14, fontWeight: "600" },
  stock: { fontSize: 12, color: "#1E7A34" },
  outOfStock: { color: "#B3261E" },
});
