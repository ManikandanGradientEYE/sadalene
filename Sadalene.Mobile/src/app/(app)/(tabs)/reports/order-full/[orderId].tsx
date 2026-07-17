import { useLocalSearchParams } from "expo-router";
import { ScrollView, StyleSheet, Text, View } from "react-native";

import { Screen, SectionLabel } from "@/components/prototype/primitives";
import { SpecTable } from "@/components/prototype/SpecTable";
import { TopBar } from "@/components/prototype/TopBar";
import { Colors, Spacing } from "@/constants/prototype-theme";
import { MOCK_PENDING_ORDERS } from "@/mock/reports";

const LEGEND = [
  "1. Customer code — kept the same in your database and ours",
  "2. Order No",
  "3. Quality",
  "4. Quantity",
  "5. Remarks",
];

export default function OrderFullDetailScreen() {
  const { orderId } = useLocalSearchParams<{ orderId: string }>();
  const order = MOCK_PENDING_ORDERS.find((o) => o.id === orderId);
  const rows = (order?.quality ?? []).map((q) => [orderId ?? "", q, "1", "—"]);

  return (
    <Screen>
      <TopBar title={`Order ${orderId}`} subtitle="Full detail" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SpecTable cols={["Order No", "Quality", "Set", "Remark"]} rows={rows} />
        <SectionLabel text="Field Legend" />
        <View style={styles.legend}>
          {LEGEND.map((item) => (
            <Text key={item} style={styles.legendItem}>
              {item}
            </Text>
          ))}
        </View>
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
  legend: { paddingHorizontal: Spacing.three, gap: 6 },
  legendItem: { fontSize: 12, color: Colors.ink, lineHeight: 17 },
});
