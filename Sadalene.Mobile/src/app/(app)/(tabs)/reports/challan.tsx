import { ScrollView } from "react-native";

import { NoteBox, Screen } from "@/components/prototype/primitives";
import { SpecTable } from "@/components/prototype/SpecTable";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_CHALLANS } from "@/mock/reports";

export default function ChallanDataScreen() {
  const rows = MOCK_CHALLANS.map((c) => [c.date, c.number]);

  return (
    <Screen>
      <TopBar title="Challan Data" subtitle="Uploaded challan reports" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SpecTable cols={["Date", "Challan No"]} rows={rows} />
        <NoteBox text="Filename format: AAA00 00056 01071984 → customer code + challan no + date." />
      </ScrollView>
    </Screen>
  );
}
