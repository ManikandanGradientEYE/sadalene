import { ScrollView } from "react-native";

import { NoteBox, Screen } from "@/components/prototype/primitives";
import { SpecTable } from "@/components/prototype/SpecTable";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_INVOICES } from "@/mock/reports";

export default function InvoiceDataScreen() {
  const rows = MOCK_INVOICES.map((c) => [c.date, c.number]);

  return (
    <Screen>
      <TopBar title="Invoice Data" subtitle="Uploaded invoice reports" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SpecTable cols={["Date", "Invoice No"]} rows={rows} />
        <NoteBox text="Filename format: AAA00 00056 01071984 → customer code + invoice no + date." />
      </ScrollView>
    </Screen>
  );
}
