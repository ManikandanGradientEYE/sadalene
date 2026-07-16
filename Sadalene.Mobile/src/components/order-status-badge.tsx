import { StyleSheet, Text, View } from "react-native";

import type { OrderStatus } from "@/types/dto";

const STATUS_COLORS: Record<OrderStatus, { bg: string; fg: string }> = {
  Pending: { bg: "#FFF4CE", fg: "#8A6D00" },
  Confirmed: { bg: "#DFF3E3", fg: "#1E7A34" },
  Processing: { bg: "#E1EDFB", fg: "#1B5FA8" },
  Dispatched: { bg: "#E5E0FA", fg: "#5B3FBF" },
  Delivered: { bg: "#DFF3E3", fg: "#1E7A34" },
  Cancelled: { bg: "#FDECEA", fg: "#B3261E" },
};

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  const colors = STATUS_COLORS[status];
  return (
    <View style={[styles.badge, { backgroundColor: colors.bg }]}>
      <Text style={[styles.text, { color: colors.fg }]}>{status}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 999,
    alignSelf: "flex-start",
  },
  text: {
    fontSize: 12,
    fontWeight: "600",
  },
});
