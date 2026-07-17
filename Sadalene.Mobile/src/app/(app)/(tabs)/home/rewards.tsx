import { useState } from "react";
import { Alert, ScrollView } from "react-native";

import { CameraBox, Screen } from "@/components/prototype/primitives";
import { RewardHero } from "@/components/prototype/RewardHero";
import { TopBar } from "@/components/prototype/TopBar";

export default function RewardsScreen() {
  const [points, setPoints] = useState(1250);

  function handleScan() {
    // UI-only pass — no real barcode scan yet, just simulates a sticker with a printed reward value.
    const reward = 100;
    setPoints((p) => p + reward);
    Alert.alert("Sticker Scanned", `You earned ₹${reward} in reward points!`);
  }

  return (
    <Screen>
      <TopBar title="Scan for Rewards" subtitle="Reward-point scanning" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <RewardHero points={points.toLocaleString("en-IN")} />
        <CameraBox label="Scan Sticker" onPress={handleScan} />
      </ScrollView>
    </Screen>
  );
}
