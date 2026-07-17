import { Stack } from "expo-router";

export default function ScanLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="index" />
      <Stack.Screen name="order-sample" />
      <Stack.Screen name="division" />
      <Stack.Screen name="category" />
      <Stack.Screen name="product/[name]" />
      <Stack.Screen name="qty-entry" />
    </Stack>
  );
}
