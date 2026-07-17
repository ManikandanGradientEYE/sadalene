import { useLocalSearchParams, useRouter } from "expo-router";
import { ScrollView } from "react-native";

import { Badge, NoteBox, PrimaryButton, Screen } from "@/components/prototype/primitives";
import { SpecTable } from "@/components/prototype/SpecTable";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_PENDING_ORDERS } from "@/mock/reports";

export default function PendingOrderDetailScreen() {
  const { orderId } = useLocalSearchParams<{ orderId: string }>();
  const router = useRouter();
  const order = MOCK_PENDING_ORDERS.find((o) => o.id === orderId);

  const rows = (order?.quality ?? []).map((q) => [orderId ?? "", q, "1", ""]);

  return (
    <Screen>
      <TopBar title={`Order ${orderId}`} subtitle="Order detail" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <Badge text="ORDER PENDING" />
        <NoteBox text="When the user taps a specific order, show them the order details below." />
        <SpecTable cols={["Order No", "Quality", "Set", "Remark"]} rows={rows} />
        <PrimaryButton text="View Full Detail" variant="ghost" onPress={() => router.push(`/reports/order-full/${orderId}`)} />
      </ScrollView>
    </Screen>
  );
}
