import { useRouter } from "expo-router";
import { useMemo } from "react";
import { ScrollView } from "react-native";

import { NoteBox, PrimaryButton, RefNoBox, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";

function makeOrderRef() {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, "0");
  const d = String(now.getDate()).padStart(2, "0");
  const seq = String(Math.floor(Math.random() * 9000) + 1000);
  return `ORD-${y}${m}${d}-${seq}`;
}

export default function OrderPlacedScreen() {
  const router = useRouter();
  const orderRef = useMemo(makeOrderRef, []);

  return (
    <Screen>
      <TopBar title="Order Placed" subtitle="Reference number" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <RefNoBox label="Order Reference No." value={orderRef} />
        <NoteBox text="Order pending for approval means the order is placed but has not yet transferred to our system." />
        <PrimaryButton text="View Pending Orders" onPress={() => router.push("/reports")} />
      </ScrollView>
    </Screen>
  );
}
