import { Ionicons } from "@expo/vector-icons";
import { Pressable, StyleSheet, Text, View } from "react-native";

import { QuantityStepper } from "@/components/quantity-stepper";
import { Spacing } from "@/constants/theme";
import type { CartItemDto } from "@/types/dto";

interface Props {
  item: CartItemDto;
  onChangeQuantity: (quantity: number) => void;
  onRemove: () => void;
}

// Full/Half is chosen at add-to-cart time (Product Detail screen, where we know allowsHalfUnit);
// CartItemDto doesn't carry that flag, so this row shows it read-only — remove and re-add to change it.
export function CartItemRow({ item, onChangeQuantity, onRemove }: Props) {
  return (
    <View style={styles.row}>
      <View style={styles.info}>
        <Text style={styles.name} numberOfLines={2}>
          {item.productName}
        </Text>
        <View style={styles.metaRow}>
          {item.productCode ? <Text style={styles.meta}>{item.productCode}</Text> : null}
          <View style={styles.unitBadge}>
            <Text style={styles.unitBadgeText}>{item.unitType}</Text>
          </View>
        </View>
        <Text style={styles.price}>
          ₹{item.displayUnitPrice.toFixed(2)} / {item.unitOfMeasure}
        </Text>
      </View>
      <View style={styles.actions}>
        <QuantityStepper value={item.quantity} onChange={onChangeQuantity} />
        <Text style={styles.lineTotal}>₹{item.lineTotal.toFixed(2)}</Text>
        <Pressable onPress={onRemove} hitSlop={8}>
          <Ionicons name="trash-outline" size={20} color="#B3261E" />
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: Spacing.two,
    paddingHorizontal: Spacing.three,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: "#E0E1E6",
    gap: Spacing.two,
  },
  info: { flex: 1, gap: 2 },
  name: { fontSize: 15, fontWeight: "600" },
  metaRow: { flexDirection: "row", alignItems: "center", gap: 6 },
  meta: { fontSize: 12, color: "#8A8F98" },
  unitBadge: { backgroundColor: "#F0F0F3", borderRadius: 999, paddingHorizontal: 8, paddingVertical: 2 },
  unitBadgeText: { fontSize: 11, fontWeight: "600", color: "#60646C" },
  price: { fontSize: 12, color: "#8A8F98" },
  actions: { alignItems: "flex-end", gap: Spacing.one },
  lineTotal: { fontSize: 14, fontWeight: "700" },
});
