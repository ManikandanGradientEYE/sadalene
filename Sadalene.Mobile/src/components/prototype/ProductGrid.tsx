import { Pressable, StyleSheet, Text, View } from "react-native";

import { Colors, Radius, Spacing } from "@/constants/prototype-theme";

export function ProductSwatchCard({ name, onPress }: { name: string; onPress?: () => void }) {
  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View style={styles.swatch} />
      <Text style={styles.name} numberOfLines={2}>
        {name}
      </Text>
    </Pressable>
  );
}

export function ProductGrid({ items, onPress }: { items: string[]; onPress?: (item: string) => void }) {
  return (
    <View style={styles.grid}>
      {items.map((item) => (
        <ProductSwatchCard key={item} name={item} onPress={() => onPress?.(item)} />
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  grid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: Spacing.two,
    paddingHorizontal: Spacing.three,
    paddingVertical: Spacing.two,
  },
  card: {
    width: "47%",
    backgroundColor: Colors.paper,
    borderWidth: 1,
    borderColor: Colors.line,
    borderRadius: Radius.md,
    overflow: "hidden",
  },
  swatch: { height: 70, backgroundColor: Colors.creamDk },
  name: { fontSize: 11, fontWeight: "700", padding: 9, color: Colors.ink },
});
