import { useState } from "react";
import { Alert, ScrollView } from "react-native";

import { ProductGrid } from "@/components/prototype/ProductGrid";
import { NoteBox, PrimaryButton, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_ADVANCE_BOOKING_PRODUCTS } from "@/mock/catalog";

export default function AdvanceBookingScreen() {
  const [selected, setSelected] = useState<string | null>(null);

  function handlePrebook() {
    if (!selected) {
      Alert.alert("Select a product", "Tap a product above to pre-book it first.");
      return;
    }
    Alert.alert("Pre-booked", `${selected} has been added to a pre-booking order.`);
    setSelected(null);
  }

  return (
    <Screen>
      <TopBar title="Advance Booking" subtitle="Pre-book upcoming stock" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <ProductGrid items={MOCK_ADVANCE_BOOKING_PRODUCTS} onPress={setSelected} />
        <NoteBox text="If the user wants to pre-book a product, an order form is created just like a normal order form." />
        <PrimaryButton text={selected ? `Pre-book ${selected}` : "Pre-book Selected"} variant="gold" onPress={handlePrebook} />
      </ScrollView>
    </Screen>
  );
}
