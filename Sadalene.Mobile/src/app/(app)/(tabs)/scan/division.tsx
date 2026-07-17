import { useRouter } from "expo-router";
import { ScrollView } from "react-native";

import { ListSection, Screen, SectionLabel } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { MOCK_DIVISIONS } from "@/mock/catalog";

export default function DivisionScreen() {
  const router = useRouter();

  return (
    <Screen>
      <TopBar title="Select Division" subtitle="Choose a division" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <SectionLabel text="Select Division" />
        <ListSection
          items={MOCK_DIVISIONS.map((d) => d.name)}
          onPress={(name) => router.push({ pathname: "/scan/category", params: { division: name } })}
        />
      </ScrollView>
    </Screen>
  );
}
