import { useRouter } from "expo-router";
import { ScrollView } from "react-native";

import { Banner, Screen } from "@/components/prototype/primitives";
import { MenuGrid } from "@/components/prototype/MenuGrid";
import { Stories } from "@/components/prototype/Stories";
import { TopBar } from "@/components/prototype/TopBar";

const MENU_ITEMS = ["Order Sample", "Raise Query", "Quick Order", "Search", "Scan for Rewards", "Advance Booking"];

export default function HomeScreen() {
  const router = useRouter();

  function handleMenuPress(item: string) {
    switch (item) {
      case "Order Sample":
        router.push("/scan/order-sample");
        break;
      case "Raise Query":
        router.push("/home/raise-query");
        break;
      case "Quick Order":
        router.push("/home/quick-order");
        break;
      case "Search":
        router.push("/home/search");
        break;
      case "Scan for Rewards":
        router.push("/home/rewards");
        break;
      case "Advance Booking":
        router.push("/home/advance-booking");
        break;
    }
  }

  return (
    <Screen>
      <TopBar title="Home" subtitle="Landing screen after login" customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <Stories />
        <Banner text="Latest Collection — New Arrivals" />
        <MenuGrid items={MENU_ITEMS} onPress={handleMenuPress} />
      </ScrollView>
    </Screen>
  );
}
