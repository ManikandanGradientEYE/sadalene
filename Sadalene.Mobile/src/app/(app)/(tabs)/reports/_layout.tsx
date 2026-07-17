import { Stack } from "expo-router";

export default function ReportsLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="index" />
      <Stack.Screen name="order/[orderId]" />
      <Stack.Screen name="order-full/[orderId]" />
      <Stack.Screen name="challan" />
      <Stack.Screen name="invoice" />
    </Stack>
  );
}
