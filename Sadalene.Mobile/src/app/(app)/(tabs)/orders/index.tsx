import { Link } from "expo-router";
import { ActivityIndicator, FlatList, Pressable, StyleSheet, Text, View } from "react-native";

import { EmptyState } from "@/components/empty-state";
import { ErrorBanner } from "@/components/error-banner";
import { OrderStatusBadge } from "@/components/order-status-badge";
import { Spacing } from "@/constants/theme";
import { useMyOrders } from "@/hooks/use-order-queries";
import { errorMessage } from "@/lib/queryClient";
import type { OrderSummaryDto } from "@/types/dto";

export default function OrdersScreen() {
  const orders = useMyOrders();

  if (orders.isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator />
      </View>
    );
  }

  if (orders.error) {
    return (
      <View style={styles.center}>
        <ErrorBanner message={errorMessage(orders.error)} />
      </View>
    );
  }

  return (
    <FlatList
      data={orders.data?.items ?? []}
      keyExtractor={(item) => String(item.id)}
      renderItem={({ item }) => <OrderRow order={item} />}
      ListEmptyComponent={<EmptyState message="No orders yet." />}
    />
  );
}

function OrderRow({ order }: { order: OrderSummaryDto }) {
  return (
    <Link href={`/orders/${order.id}`} asChild>
      <Pressable style={styles.row}>
        <View style={styles.rowInfo}>
          <Text style={styles.orderNumber}>{order.orderNumber}</Text>
          <Text style={styles.meta}>
            {order.customerName} · {new Date(order.orderDate).toLocaleDateString()}
          </Text>
          <Text style={styles.meta}>{order.itemCount} item(s)</Text>
        </View>
        <View style={styles.rowRight}>
          <OrderStatusBadge status={order.status} />
          <Text style={styles.total}>₹{order.grandTotal.toFixed(2)}</Text>
        </View>
      </Pressable>
    </Link>
  );
}

const styles = StyleSheet.create({
  center: { flex: 1, alignItems: "center", justifyContent: "center" },
  row: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingHorizontal: Spacing.three,
    paddingVertical: Spacing.two,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: "#E0E1E6",
  },
  rowInfo: { gap: 2 },
  orderNumber: { fontSize: 15, fontWeight: "700" },
  meta: { fontSize: 12, color: "#8A8F98" },
  rowRight: { alignItems: "flex-end", gap: 6 },
  total: { fontSize: 14, fontWeight: "600" },
});
