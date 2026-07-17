import { useState } from "react";
import { Alert, FlatList, Modal, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { DropdownBox, NoteBox, Screen } from "@/components/prototype/primitives";
import { TopBar } from "@/components/prototype/TopBar";
import { Colors, Radius, Spacing } from "@/constants/prototype-theme";
import { MOCK_PRODUCTS_BY_TAB } from "@/mock/catalog";

const ALL_PRODUCTS = Object.values(MOCK_PRODUCTS_BY_TAB).flat();

export default function QuickOrderScreen() {
  const [pickerOpen, setPickerOpen] = useState(false);

  function selectProduct(name: string) {
    setPickerOpen(false);
    // UI-only pass — mock add-to-cart confirmation, no cart state wired yet.
    Alert.alert("Added to Cart", `${name} was added to your cart.`);
  }

  return (
    <Screen>
      <TopBar title="Quick Order" subtitle="Dropdown-based ordering" back customerBadge="RAJ TEXTILES" />
      <ScrollView>
        <DropdownBox text="Select a product…" onPress={() => setPickerOpen(true)} />
        <NoteBox text="Item is added to cart as soon as the user selects it from the dropdown." />
      </ScrollView>

      <Modal visible={pickerOpen} animationType="slide" transparent onRequestClose={() => setPickerOpen(false)}>
        <Pressable style={styles.backdrop} onPress={() => setPickerOpen(false)}>
          <View style={styles.sheet}>
            <Text style={styles.sheetTitle}>Select a Product</Text>
            <FlatList
              data={ALL_PRODUCTS}
              keyExtractor={(item) => item}
              renderItem={({ item }) => (
                <Pressable style={styles.option} onPress={() => selectProduct(item)}>
                  <Text style={styles.optionText}>{item}</Text>
                </Pressable>
              )}
            />
          </View>
        </Pressable>
      </Modal>
    </Screen>
  );
}

const styles = StyleSheet.create({
  backdrop: { flex: 1, backgroundColor: "rgba(0,0,0,0.4)", justifyContent: "flex-end" },
  sheet: {
    backgroundColor: Colors.paper,
    borderTopLeftRadius: Radius.xl,
    borderTopRightRadius: Radius.xl,
    maxHeight: "70%",
    padding: Spacing.three,
  },
  sheetTitle: { fontSize: 16, fontWeight: "700", marginBottom: Spacing.two, color: Colors.ink },
  option: { paddingVertical: 12, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: Colors.line },
  optionText: { fontSize: 14, color: Colors.ink },
});
