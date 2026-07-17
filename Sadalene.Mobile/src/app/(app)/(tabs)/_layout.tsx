import { Redirect, Tabs } from "expo-router";
import { StyleSheet, Text, type ColorValue } from "react-native";

import { useActingCustomer } from "@/actingCustomer/useActingCustomer";
import { useAuth } from "@/auth/useAuth";
import { Colors, Fonts } from "@/constants/prototype-theme";

const ICONS: Record<string, string> = {
  home: "⌂",
  scan: "▣",
  gallery: "▦",
  cart: "⛃",
  reports: "▤",
};

function TabIcon({ name, color }: { name: string; color: ColorValue }) {
  return <Text style={[styles.icon, { color }]}>{ICONS[name]}</Text>;
}

export default function TabsLayout() {
  const { auth } = useAuth();
  const { actingCustomer } = useActingCustomer();

  // Agent/Staff must pick who they're acting for before entering the main app. (Not reachable in
  // this UI-only pass since mock sign-in always uses identityType "Customer" — kept for when the
  // real auth API is wired back in.)
  const needsPicker = auth!.identityType !== "Customer" && !actingCustomer;
  if (needsPicker) return <Redirect href="/customer-picker" />;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: Colors.maroon,
        tabBarInactiveTintColor: Colors.grey,
        tabBarStyle: { backgroundColor: Colors.paper, borderTopColor: Colors.line },
        tabBarLabelStyle: { fontSize: 8.7, fontWeight: "700" },
      }}
    >
      <Tabs.Screen
        name="home"
        options={{ title: "Home", tabBarIcon: ({ color }) => <TabIcon name="home" color={color} /> }}
      />
      <Tabs.Screen
        name="scan"
        options={{ title: "Scan to Order", tabBarIcon: ({ color }) => <TabIcon name="scan" color={color} /> }}
      />
      <Tabs.Screen
        name="gallery"
        options={{ title: "Gallery", tabBarIcon: ({ color }) => <TabIcon name="gallery" color={color} /> }}
      />
      <Tabs.Screen
        name="cart"
        options={{ title: "View Cart", tabBarIcon: ({ color }) => <TabIcon name="cart" color={color} /> }}
      />
      <Tabs.Screen
        name="reports"
        options={{ title: "Reports", tabBarIcon: ({ color }) => <TabIcon name="reports" color={color} /> }}
      />
    </Tabs>
  );
}

const styles = StyleSheet.create({
  icon: { fontSize: 16, fontFamily: Fonts.display },
});
