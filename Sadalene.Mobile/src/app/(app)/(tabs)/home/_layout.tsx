import { Stack } from "expo-router";

export default function HomeLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="index" />
      <Stack.Screen name="raise-query" />
      <Stack.Screen name="quick-order" />
      <Stack.Screen name="search" />
      <Stack.Screen name="rewards" />
      <Stack.Screen name="advance-booking" />
    </Stack>
  );
}
