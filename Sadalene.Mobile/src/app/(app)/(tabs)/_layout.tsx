import { Ionicons } from "@expo/vector-icons";
import { Redirect, Tabs } from "expo-router";

import { useActingCustomer } from "@/actingCustomer/useActingCustomer";
import { useAuth } from "@/auth/useAuth";

export default function TabsLayout() {
  const { auth } = useAuth();
  const { actingCustomer } = useActingCustomer();

  // Agent/Staff must pick who they're acting for before entering the main app.
  const needsPicker = auth!.identityType !== "Customer" && !actingCustomer;
  if (needsPicker) return <Redirect href="/customer-picker" />;

  return (
    <Tabs>
      <Tabs.Screen
        name="catalog"
        options={{
          title: "Catalog",
          headerShown: false,
          tabBarIcon: ({ color, size }) => <Ionicons name="grid-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen
        name="scan"
        options={{
          title: "Scan",
          tabBarIcon: ({ color, size }) => <Ionicons name="barcode-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen
        name="cart"
        options={{
          title: "Cart",
          tabBarIcon: ({ color, size }) => <Ionicons name="cart-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen
        name="orders"
        options={{
          title: "Orders",
          headerShown: false,
          tabBarIcon: ({ color, size }) => <Ionicons name="receipt-outline" size={size} color={color} />,
        }}
      />
    </Tabs>
  );
}
