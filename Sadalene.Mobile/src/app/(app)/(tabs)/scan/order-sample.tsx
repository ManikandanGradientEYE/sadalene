import { useState } from "react";
import { Alert, ScrollView } from "react-native";

import { CameraBox, NoteBox, PrimaryButton, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";

export default function OrderSampleScreen() {
  const [sampleCount, setSampleCount] = useState(0);

  function handleScan() {
    setSampleCount((c) => c + 1);
  }

  return (
    <Screen>
      <TopBar title="Order Sample" subtitle="Free sample request" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <CameraBox label="Open Camera" onPress={handleScan} />
        <NoteBox text="Scan a product to add it to your sample order." />
        <PrimaryButton
          text={sampleCount > 0 ? `Add to Sample Order (${sampleCount})` : "Add to Sample Order"}
          variant="gold"
          onPress={() => Alert.alert("Sample Order", `${sampleCount} sample(s) queued for request.`)}
        />
      </ScrollView>
    </Screen>
  );
}
