import { Stack } from "expo-router";

export default function CatalogLayout() {
  return (
    <Stack>
      <Stack.Screen name="index" options={{ title: "Catalog" }} />
      <Stack.Screen name="[productId]" options={{ title: "Product" }} />
    </Stack>
  );
}
