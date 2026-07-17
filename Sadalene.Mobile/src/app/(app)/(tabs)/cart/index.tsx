import { useRouter } from "expo-router";
import { ScrollView } from "react-native";

import { EmptyState } from "@/components/empty-state";
import { PrimaryButton, Screen, SectionLabel, TotalRow } from "@/components/prototype/primitives";
import { SpecTable } from "@/components/prototype/SpecTable";
import { TopBar } from "@/components/prototype/TopBar";
import { useMockCart } from "@/mock/MockCartContext";

export default function OrderSummaryScreen() {
  const { lines, clear } = useMockCart();
  const router = useRouter();

  const totalPieces = lines.reduce((sum, l) => sum + l.qty, 0);
  const rows = lines.map((l, i) => [String(i + 1), l.option ? `${l.product} (${l.option})` : l.product, String(l.qty)]);

  function handlePlaceOrder() {
    clear();
    router.push("/cart/placed");
  }

  return (
    <Screen>
      <TopBar title="Order Summary" subtitle="Cart / place order" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SectionLabel text="Order No …………" />
        {lines.length === 0 ? (
          <EmptyState message="Your cart is empty. Browse the catalog or scan a product to add items." />
        ) : (
          <>
            <SpecTable cols={["S.No", "Product", "Qty"]} rows={rows} />
            <TotalRow label="Total Pieces" value={String(totalPieces)} />
            <PrimaryButton text="Place Order" variant="gold" onPress={handlePlaceOrder} />
          </>
        )}
      </ScrollView>
    </Screen>
  );
}
