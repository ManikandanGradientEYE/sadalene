import { useLocalSearchParams } from "expo-router";
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from "react-native";

import { ErrorBanner } from "@/components/error-banner";
import { OrderStatusBadge } from "@/components/order-status-badge";
import { Spacing } from "@/constants/theme";
import { useOrder } from "@/hooks/use-order-queries";
import { errorMessage } from "@/lib/queryClient";

export default function OrderDetailScreen() {
  const { orderId } = useLocalSearchParams<{ orderId: string }>();
  const id = Number(orderId);
  const order = useOrder(Number.isFinite(id) ? id : null);

  if (order.isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (order.error || !order.data) {
    return (
      <View style={styles.center}>
        <ErrorBanner message={order.error ? errorMessage(order.error) : "Order not found."} />
      </View>
    );
  }

  const o = order.data;

  return (
    <ScrollView contentContainerStyle={styles.container}>
      <View style={styles.header}>
        <Text style={styles.orderNumber}>{o.orderNumber}</Text>
        <OrderStatusBadge status={o.status} />
      </View>
      <Text style={styles.meta}>{new Date(o.orderDate).toLocaleString()}</Text>
      <Text style={styles.meta}>Customer: {o.customerName}</Text>
      {o.agentName ? <Text style={styles.meta}>Agent: {o.agentName}</Text> : null}
      {o.placedByName ? <Text style={styles.meta}>Placed by: {o.placedByName}</Text> : null}
      {o.notes ? <Text style={styles.notes}>{o.notes}</Text> : null}

      <View style={styles.itemsHeader}>
        <Text style={styles.itemsTitle}>Items</Text>
      </View>
      {o.items.map((item, index) => (
        <View key={index} style={styles.row}>
          <View style={styles.rowInfo}>
            <Text style={styles.itemName} numberOfLines={2}>
              {item.productName}
            </Text>
            <Text style={styles.meta}>
              {item.productCode ?? ""} · {item.unitType} · x{item.quantity}
            </Text>
          </View>
          <Text style={styles.itemTotal}>₹{item.lineTotal.toFixed(2)}</Text>
        </View>
      ))}

      <View style={styles.totalRow}>
        <Text style={styles.totalLabel}>Grand Total</Text>
        <Text style={styles.totalValue}>₹{o.grandTotal.toFixed(2)}</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { padding: Spacing.three, gap: Spacing.one },
  center: { flex: 1, alignItems: "center", justifyContent: "center" },
  header: { flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  orderNumber: { fontSize: 20, fontWeight: "700" },
  meta: { fontSize: 13, color: "#8A8F98" },
  notes: { fontSize: 14, color: "#3C3F44", marginTop: Spacing.one },
  itemsHeader: { marginTop: Spacing.three, marginBottom: Spacing.one },
  itemsTitle: { fontSize: 16, fontWeight: "700" },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: Spacing.two,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: "#E0E1E6",
    gap: Spacing.two,
  },
  rowInfo: { flex: 1, gap: 2 },
  itemName: { fontSize: 14, fontWeight: "600" },
  itemTotal: { fontSize: 14, fontWeight: "700" },
  totalRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginTop: Spacing.three,
    paddingTop: Spacing.two,
    borderTopWidth: StyleSheet.hairlineWidth,
    borderTopColor: "#E0E1E6",
  },
  totalLabel: { fontSize: 16, fontWeight: "700" },
  totalValue: { fontSize: 18, fontWeight: "700" },
});
