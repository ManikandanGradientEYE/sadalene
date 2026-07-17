import { useRouter } from "expo-router";
import { ScrollView } from "react-native";

import { ListSection, Screen, SectionLabel, TwoButtonRow } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_PENDING_ORDERS } from "@/mock/reports";

export default function PendingOrdersScreen() {
  const router = useRouter();

  return (
    <Screen>
      <TopBar title="Pending Orders" subtitle="Awaiting approval" customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SectionLabel text="Order Pending for Approval" />
        <ListSection
          items={MOCK_PENDING_ORDERS.map((o) => o.label)}
          onPress={(_label, i) => router.push(`/reports/order/${MOCK_PENDING_ORDERS[i].id}`)}
        />
        <TwoButtonRow
          a="View Challan"
          b="View Invoice"
          onA={() => router.push("/reports/challan")}
          onB={() => router.push("/reports/invoice")}
        />
      </ScrollView>
    </Screen>
  );
}
