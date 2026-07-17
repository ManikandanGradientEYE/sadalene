import { Alert, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { EmptyState } from "@/components/empty-state";
import { PrimaryButton, Qtext, Screen, SectionLabel } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { Colors, Spacing } from "@/constants/prototype-theme";
import { useMockCart } from "@/mock/MockCartContext";

export default function QtyEntryScreen() {
  const { lines, updateQty } = useMockCart();

  return (
    <Screen>
      <TopBar title="Quantity Entry" subtitle="Add / adjust item quantities" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        {lines.length === 0 ? (
          <EmptyState message="No items yet — scan or browse a product to add one." />
        ) : (
          <View style={styles.table}>
            <View style={styles.headerRow}>
              <Text style={[styles.th, { flex: 0.4 }]}>S.No</Text>
              <Text style={[styles.th, { flex: 1.4 }]}>Product</Text>
              <Text style={[styles.th, { flex: 1 }]}>Qty</Text>
            </View>
            {lines.map((line, i) => (
              <View key={line.id} style={[styles.row, i % 2 === 1 && styles.rowAlt]}>
                <Text style={[styles.td, { flex: 0.4 }]}>{i + 1}</Text>
                <Text style={[styles.td, { flex: 1.4 }]} numberOfLines={2}>
                  {line.product}
                  {line.option ? ` (${line.option})` : ""}
                </Text>
                <View style={[styles.qtyCell, { flex: 1 }]}>
                  <Pressable style={styles.stepBtn} onPress={() => updateQty(line.id, line.qty - 1)}>
                    <Text style={styles.stepBtnText}>−</Text>
                  </Pressable>
                  <Text style={styles.qtyValue}>{line.qty}</Text>
                  <Pressable style={styles.stepBtn} onPress={() => updateQty(line.id, line.qty + 1)}>
                    <Text style={styles.stepBtnText}>+</Text>
                  </Pressable>
                </View>
              </View>
            ))}
          </View>
        )}

        <SectionLabel text="Description" />
        <Qtext text="Qty per unit — scroll to view full matching detail and cut information." />

        <PrimaryButton
          text="Print Order (if printer connected)"
          variant="ghost"
          onPress={() => Alert.alert("Print Order", "No printer connected.")}
        />
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
  table: {
    marginHorizontal: Spacing.three,
    marginTop: Spacing.two,
    borderWidth: 1,
    borderColor: Colors.line,
    borderRadius: 8,
    overflow: "hidden",
  },
  headerRow: { flexDirection: "row", backgroundColor: Colors.maroon },
  th: { color: Colors.goldLt, fontSize: 9.5, textTransform: "uppercase", fontWeight: "700", padding: 8 },
  row: { flexDirection: "row", alignItems: "center", borderTopWidth: 1, borderTopColor: Colors.line },
  rowAlt: { backgroundColor: "rgba(228,216,190,0.35)" },
  td: { fontSize: 11, color: Colors.ink, padding: 8 },
  qtyCell: { flexDirection: "row", alignItems: "center", gap: 6, paddingVertical: 6 },
  stepBtn: {
    width: 20,
    height: 20,
    borderRadius: 5,
    backgroundColor: Colors.maroon,
    alignItems: "center",
    justifyContent: "center",
  },
  stepBtnText: { color: "#fff", fontSize: 12, fontWeight: "700" },
  qtyValue: { fontSize: 12, fontWeight: "700", color: Colors.ink, minWidth: 18, textAlign: "center" },
});
